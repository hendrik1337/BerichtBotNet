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

    private readonly GroupView _groupView;
    private readonly ApprenticeView _apprenticeView;

    public GroupController(GroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
        
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
        }
    }

    private async Task AddGroup(SocketModal modal)
    {
        try
        {
            var groupName = AddGroupFromModal(modal, out var group);
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
            var groupName = AddGroupFromModal(modal, out var group);

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
    public string AddGroupFromModal(SocketModal modal, out Group group)
    {
        List<SocketMessageComponentData> components =
            modal.Data.Components.ToList();

        string groupName = components.First(x => x.CustomId == "group_name").Value;
        string groupId = components.First(x => x.CustomId == "group_id").Value;
        string groupStart = components.First(x => x.CustomId == "group_start").Value;
        string groupTime = components.First(x => x.CustomId == "group_time").Value;
        string groupDay = components.First(x => x.CustomId == "group_day").Value;

        DayOfWeek groupReminderWeekday;


        groupReminderWeekday = GroupReminderWeekday(groupDay);


        DateTime groupReminderTime = DateTime.Parse(groupTime, Constants.CultureInfo).ToUniversalTime();
        DateTime dateTimeGroupStart = DateTime.Parse(groupStart, Constants.CultureInfo).ToUniversalTime();

        group = new Group()
        {
            Name = groupName,
            StartOfApprenticeship = dateTimeGroupStart,
            DiscordGroupId = groupId,
            ReminderTime = groupReminderTime,
            ReminderWeekDay = groupReminderWeekday
        };
        return groupName;
    }

    private DayOfWeek GroupReminderWeekday(string groupDay)
    {
        DayOfWeek groupReminderWeekday;
        switch (groupDay.ToLower())
        {
            case "montag":
                groupReminderWeekday = DayOfWeek.Monday;
                break;
            case "dienstag":
                groupReminderWeekday = DayOfWeek.Tuesday;
                break;
            case "mittwoch":
                groupReminderWeekday = DayOfWeek.Wednesday;
                break;
            case "donnerstag":
                groupReminderWeekday = DayOfWeek.Thursday;
                break;
            case "freitag":
                groupReminderWeekday = DayOfWeek.Friday;
                break;
            case "samstag":
                groupReminderWeekday = DayOfWeek.Saturday;
                break;
            case "sonntag":
                groupReminderWeekday = DayOfWeek.Sunday;
                break;
            default:
                throw new InvalidWeekDayInputException();
        }

        return groupReminderWeekday;
    }

    
}