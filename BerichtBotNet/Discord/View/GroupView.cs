using System.Reflection.Metadata;
using BerichtBotNet.Data;
using BerichtBotNet.Helper;
using Discord;
using Discord.WebSocket;

namespace BerichtBotNet.Discord.View;

public class GroupView
{
    public ModalBuilder GetAddGroupModal(ulong channelId)
    {
        return new ModalBuilder()
            .WithTitle("Gruppe Hinzufügen")
            .AddTextInput("Name", "group_name", placeholder: "FI-22")
            .AddTextInput("Ausbildungsstart", "group_start", placeholder: "03.08.2022")
            .AddTextInput("Berichtsheft Erinnerungs Tag und Uhrzeit", "group_time", placeholder: "Mo, 08:30")
            .AddTextInput("Discord Kanal Id (Standard aktueller Kanal)", "group_id",
                placeholder: "Channel für Wochentliche Erinnerungen",
                required: true, value: channelId.ToString());
    }

    public ModalBuilder GetEditGroupModal(Group group)
    {
        string weekdayTimeCombination = "";
        weekdayTimeCombination += WeekHelper.ParseDayOfWeekIntoString(group.ReminderDayOfWeek);
        weekdayTimeCombination += ", ";
        weekdayTimeCombination += group.ReminderTime.ToLocalTime().ToString("t", Constants.CultureInfo);
        return new ModalBuilder()
            .WithTitle("Gruppe Bearbeiten")
            .AddTextInput("Name", "group_name", placeholder: "FI-22", value: group.Name)
            .AddTextInput("Ausbildungsstart", "group_start", placeholder: "03.08.2022",
                value: group.StartOfApprenticeship.ToString("d", Constants.CultureInfo))
            .AddTextInput("Berichtsheft Erinnerungs Tag und Uhrzeit", "group_time", placeholder: "Mo, 08:30",
                value: weekdayTimeCombination)
            .AddTextInput("Discord Kanal Id", "group_id",
                placeholder: "Channel für Wochentliche Erinnerungen",
                required: true, value: group.DiscordGroupId);
    }


    // Adds group via direct slash command
    public async void AddGroup(SocketSlashCommand command)
    {
        var addApprenticeModal = GetAddGroupModal(command.Channel.Id);
        addApprenticeModal.WithCustomId("addGroupMenu");
        await command.RespondWithModalAsync(addApprenticeModal.Build());
    }

    // Adds group via MessageComponent
    public async void AddGroup(SocketMessageComponent component)
    {
        var addApprenticeModal = GetAddGroupModal(component.Channel.Id);
        addApprenticeModal.WithCustomId("addGroupMenuAndApprentice");
        await component.RespondWithModalAsync(addApprenticeModal.Build());
    }

    public async void EditGroup(Group requesterGroup, SocketSlashCommand command)
    {
        var editModal = GetEditGroupModal(requesterGroup);
        editModal.WithCustomId("editGroupMenu");
        await command.RespondWithModalAsync(editModal.Build());
    }
    
    public async void SendApprenticeRemoveConfirmation(SocketSlashCommand command, string groupName)
    {
        // Build the confirmation button
        var builder = new ComponentBuilder()
            .WithButton("Entfernen", "deleteGroup", style: ButtonStyle.Danger);

        await command.RespondAsync(
            $"Sicher, dass du die Gruppe {groupName} Entfernen möchtest?\n Dein Account wird dabei auch gelöscht.",
            components: builder.Build());
    }
}