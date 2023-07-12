using BerichtBotNet.Data;
using BerichtBotNet.Discord.View;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Constants = BerichtBotNet.Helper.Constants;

namespace BerichtBotNet.Discord;

public class GroupController
{
    private readonly GroupRepository _groupRepository;
    private readonly ApprenticeRepository _apprenticeRepository;

    private readonly GroupView _groupView;
    private readonly ApprenticeView _apprenticeView;

    public GroupController(GroupRepository groupRepository, ApprenticeRepository apprenticeRepository)
    {
        _groupRepository = groupRepository;
        _apprenticeRepository = apprenticeRepository;

        _groupView = new GroupView();
        _apprenticeView = new ApprenticeView();
    }

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
                break;
        }
    }

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
        string groupTime = components.First(x => x.CustomId == "group_time").Value;


        DateTime groupReminderTime = DateTime.Parse(groupTime, Constants.CultureInfo).ToUniversalTime();
        DateTime dateTimeGroupStart = DateTime.Parse(groupStart, Constants.CultureInfo).ToUniversalTime();

        group = new Group()
        {
            Name = groupName,
            DiscordGroupId = groupId,
            StartOfApprenticeship = dateTimeGroupStart,
            ReminderTime = groupReminderTime,
        };
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
        var groupName = CreateGroupFromModalWithoutId(modal, out var group);

        oldGroup.Name = group.Name;
        oldGroup.DiscordGroupId = group.DiscordGroupId;
        oldGroup.StartOfApprenticeship = group.StartOfApprenticeship.ToUniversalTime();
        oldGroup.ReminderTime = group.ReminderTime.ToUniversalTime();

        _groupRepository.UpdateGroup(oldGroup);

        await modal.RespondAsync($"Gruppe: {group.Name} wurde aktualisiert");
    }
}