﻿using BerichtBotNet.Data;
using BerichtBotNet.Discord.View;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Helper;
using BerichtBotNet.Repositories;
using Discord.WebSocket;
using Quartz;
using Constants = BerichtBotNet.Helper.Constants;

namespace BerichtBotNet.Discord.Controller;

public class GroupController
{
    private readonly GroupRepository _groupRepository;
    private readonly ApprenticeRepository _apprenticeRepository;
    private readonly ReminderHelper _reminderHelper;
    
    private readonly GroupView _groupView;
    private readonly ApprenticeView _apprenticeView;

    public GroupController(GroupRepository groupRepository, ApprenticeRepository apprenticeRepository, ReminderHelper reminderHelper)
    {
        _groupRepository = groupRepository;
        _apprenticeRepository = apprenticeRepository;
        _reminderHelper = reminderHelper;
        

        _groupView = new GroupView();
        _apprenticeView = new ApprenticeView();
    }
    
    // "Get"
    public void GroupCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Options.First().Value)
        {
            case "add":
                _groupView.AddGroup(command);
                break;
            case "edit":
                EditGroup(command);
                break;
            case "remove":
                DeleteGroup(command);
                break;
        }
    }

    // "Post / Update"
    public async void GroupModalHandler(SocketModal modal)
    {
        switch (modal.Data.CustomId)
        {
            case "addGroupMenu":
                await AddGroup(modal);
                break;
            case "addGroupMenuAndApprentice":
                await AddGroupAndApprentice(modal);
                break;
            case "editGroupMenu":
                await EditGroupFromModal(modal);
                break;
        }
    }
    
    // "Delete"
    public async void GroupMessageComponentHandler(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "deleteGroup":
                RemoveGroupAndLastApprentice(component);
                break;
        }
    }

    private async void RemoveGroupAndLastApprentice(SocketMessageComponent component)
    {
        var requester = _apprenticeRepository.GetApprenticeByDiscordId(component.User.Id.ToString());
        var groupName = requester.Group.Name;
        if (!ValidateRequest(component, requester).Result) return;

        var apprentices = _apprenticeRepository.GetApprenticesInSameGroupByGroupId(requester.Group.Id);
        if (apprentices.Count > 1)
        {
            await component.RespondAsync(
                $"Die Gruppe: {requester.Group.Name} kann nicht gelöscht werden.\nEs sind noch andere Azubis in der Gruppe.");
            return;
        }
        
        _groupRepository.DeleteGroup(requester.Group.Id);
        component.RespondAsync($"Die Gruppe: {groupName} wurde gelöscht");
    }


    private async Task<bool> ValidateRequest<T>(T command, Apprentice? requester)
    {
        if (requester is null)
        {
            if (command is SocketSlashCommand slashCommand)
                await slashCommand.RespondAsync(Constants.UserNotRegistered);
            else if (command is SocketMessageComponent messageComponent)
                await messageComponent.RespondAsync(Constants.UserNotRegistered);
            else if (command is SocketModal modal)
                await modal.RespondAsync(Constants.UserNotRegistered);

            return false;
        }

        return true;
    }

    private async Task AddGroup(SocketModal modal)
    {
        try
        {
            var groupName = CreateGroupFromModalWithoutId(modal, out var group);
            if (_groupRepository.GetGroupByName(groupName) == null)
            {
                _groupRepository.CreateGroup(group);
                await modal.RespondAsync($"Gruppe: {group.Name} wurde hinzugefügt");
            }
            else
            {
                await modal.RespondAsync($"Gruppe: {group.Name} existiert bereits");
            }
        }
        catch (InvalidWeekDayInputException e)
        {
            await modal.RespondAsync("Wochentag wurde nicht erkannt. Mögliche Eingaben: Montag, Dienstag,...");
        }
    }

    // This function Adds a Group and restarts the AddApprentice Routine
    private async Task AddGroupAndApprentice(SocketModal modal)
    {
        try
        {
            var groupName = CreateGroupFromModalWithoutId(modal, out var group);

            if (_groupRepository.GetGroupByName(groupName) == null)
            {
                _groupRepository.CreateGroup(group);

                List<Group>? groups = _groupRepository.GetAllGroups();

                string response = $"Gruppe: {groupName} wurde hinzugefügt\n\n\nWähle jetzt deine Gruppe aus.";
                _apprenticeView.SendGroupSelectorDropdown(modal, groups, response);
                // ApprenticeCommands.SendGroupSelector(modal, groupName);
            }
            else
            {
                await modal.RespondAsync($"Gruppe: {group.Name} existiert bereits");
            }
        }
        catch (InvalidWeekDayInputException e)
        {
            await modal.RespondAsync("Wochentag wurde nicht erkannt. Mögliche Eingaben: Montag, Dienstag,...");
        }
    }

    // This Function only adds a group
    public string CreateGroupFromModalWithoutId(SocketModal modal, out Group group)
    {
        List<SocketMessageComponentData> components =
            modal.Data.Components.ToList();

        string groupName = components.First(x => x.CustomId == "group_name").Value;
        string groupId = components.First(x => x.CustomId == "group_id").Value;
        string groupStart = components.First(x => x.CustomId == "group_start").Value;
        string groupWeekdayAndTime = components.First(x => x.CustomId == "group_time").Value;

        string groupDayOfWeekStr = groupWeekdayAndTime.Split(",")[0];
        string groupTime = groupWeekdayAndTime.Split(",")[1];

        DayOfWeek groupDayOfWeek = WeekHelper.ParseStringIntoDayOfWeek(groupDayOfWeekStr);


        DateTime groupReminderTime = DateTime.Parse(groupTime, Constants.CultureInfo).ToUniversalTime();
        DateTime dateTimeGroupStart = DateTime.Parse(groupStart, Constants.CultureInfo).ToUniversalTime();

        group = new Group()
        {
            Name = groupName,
            DiscordGroupId = groupId,
            StartOfApprenticeship = dateTimeGroupStart,
            ReminderTime = groupReminderTime,
            ReminderDayOfWeek = groupDayOfWeek
        };
        
        // Update / Create Reminder Schedules
        _reminderHelper.CreateReminderForGroup(group);
        
        
        return groupName;
    }

    private async void EditGroup(SocketSlashCommand command)
    {
        var requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        if (!ValidateRequest(command, requester).Result) return;

        _groupView.EditGroup(requester.Group, command);
    }

    private async Task EditGroupFromModal(SocketModal modal)
    {
        var requester = _apprenticeRepository.GetApprenticeByDiscordId(modal.User.Id.ToString());
        var oldGroup = requester.Group;
        var _ = CreateGroupFromModalWithoutId(modal, out var group);
        

        oldGroup.Name = group.Name;
        oldGroup.DiscordGroupId = group.DiscordGroupId;
        oldGroup.StartOfApprenticeship = group.StartOfApprenticeship.ToUniversalTime();
        oldGroup.ReminderTime = group.ReminderTime.ToUniversalTime();
        oldGroup.ReminderDayOfWeek = group.ReminderDayOfWeek;

        _groupRepository.UpdateGroup(oldGroup);

        await modal.RespondAsync($"Gruppe: {group.Name} wurde aktualisiert");
    }

    private async Task DeleteGroup(SocketSlashCommand command)
    {
        var requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        if (!ValidateRequest(command, requester).Result) return;

        var apprentices = _apprenticeRepository.GetApprenticesInSameGroupByGroupId(requester.Group.Id);
        if (apprentices.Count > 1)
        {
            await command.RespondAsync(
                $"Die Gruppe: {requester.Group.Name} kann nicht gelöscht werden.\nEs sind noch andere Azubis in der Gruppe.");
            return;
        }
        
        _groupView.SendApprenticeRemoveConfirmation(command, requester.Group.Name);
    }
}