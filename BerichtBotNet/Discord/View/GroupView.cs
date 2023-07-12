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
            .AddTextInput("Berichtsheft Erinnerungs Uhrzeit", "group_time", placeholder: "08:30")
            .AddTextInput("Discord Kanal Id (Standard aktueller Kanal)", "group_id", placeholder: "Channel für Wochentliche Erinnerungen",
                required: true, value: channelId.ToString());
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
}