using BerichtBotNet.Data;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using Constants = BerichtBotNet.Helper.Constants;

namespace BerichtBotNet.Discord;

public class GroupCommands
{
    public static void GroupCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Options.First().Value)
        {
            case "add":
                AddGroup(command);
                break;
            case "edit":
                break;
            case "remove":
                break;
        }
    }

    public static async void GroupModalHandler(SocketModal modal)
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

    private static async Task AddGroup(SocketModal modal)
    {
        using BerichtBotContext context = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(context);

        try
        {
            var groupName = AddGroupFromModal(modal, out var group);
            if (groupRepository.GetGroupByName(groupName) == null)
            {
                groupRepository.CreateGroup(group);
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
    private static async Task AddGroupAndApprentice(SocketModal modal)
    {
        using BerichtBotContext context = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(context);

        try
        {
            var groupName = AddGroupFromModal(modal, out var group);

            if (groupRepository.GetGroupByName(groupName) == null)
            {
                groupRepository.CreateGroup(group);
                ApprenticeCommands.SendGroupSelector(modal, groupName);
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
    private static string AddGroupFromModal(SocketModal modal, out Group group)
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

    private static DayOfWeek GroupReminderWeekday(string groupDay)
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

    private static ModalBuilder GetAddGroupModal(ulong channelId)
    {
        return new ModalBuilder()
            .WithTitle("Gruppe Hinzufügen")
            .AddTextInput("Name", "group_name", placeholder: "FI-22")
            .AddTextInput("Ausbildungsstart", "group_start", placeholder: "03.08.2022")
            .AddTextInput("Berichtsheft Erinnerungs Uhrzeit", "group_time", placeholder: "08:30")
            .AddTextInput("Berichtsheft Erinnerungs Tag", "group_day", placeholder: "Montag / Dienstag / Mittwoch /...")
            .AddTextInput("Discord Kanal Id (Standard aktueller Kanal)", "group_id", placeholder: "Channel für Wochentliche Erinnerungen",
                required: true, value: channelId.ToString());
    }

    // Adds group via direct slash command
    private static async void AddGroup(SocketSlashCommand command)
    {
        var addApprenticeModal = GetAddGroupModal(command.Channel.Id);
        addApprenticeModal.WithCustomId("addGroupMenu");
        await command.RespondWithModalAsync(addApprenticeModal.Build());
    }

    // Adds group via MessageComponent
    public static async void AddGroup(SocketMessageComponent component)
    {
        var addApprenticeModal = GetAddGroupModal(component.Channel.Id);
        addApprenticeModal.WithCustomId("addGroupMenuAndApprentice");
        await component.RespondWithModalAsync(addApprenticeModal.Build());
    }
}