using BerichtBotNet.Data;
using Discord;
using Discord.WebSocket;

namespace BerichtBotNet.Discord.View;

public class ApprenticeView
{
    public async void SendEditApprenticeModal(SocketSlashCommand command, Apprentice? user)
    {
        if (user is not null)
        {
            // Build and send the edit apprentice modal
            var addApprenticeModal = new ModalBuilder()
                .WithTitle("Azubi Bearbeiten")
                .WithCustomId("editApprenticeMenu")
                .AddTextInput("Name", "azubi_name", value: user.Name)
                .AddTextInput("Discord Id", "azubi_id", value: user.DiscordUserId)
                .AddTextInput("Gruppe", "azubi_group", value: user.Group?.Name,
                    placeholder: "Stelle sicher, dass die Gruppe existiert.")
                .AddTextInput("Überspringen", "azubi_skip", value: user.Skipped.ToString(),
                    placeholder: "0 = Wird nicht übersprungen, 1 = wird übersprungen");

            await command.RespondWithModalAsync(addApprenticeModal.Build());
        }
        else
        {
            await command.RespondAsync("Benutzer konnte nicht gefunden werden.");
        }
    }

    public async void SendAddApprentice(SocketMessageComponent modal, string groupId)
    {
        // Build and send the add apprentice modal with pre-filled values and a hidden group ID
        var addApprenticeModal = new ModalBuilder()
            .WithTitle("Azubi Hinzufügen")
            .WithCustomId("addApprenticeMenu")
            .AddTextInput("Name", "azubi_name", value: modal.User.Username)
            .AddTextInput("Discord Id", "azubi_id", value: modal.User.Id.ToString())
            .AddTextInput("Group Id, NICHT ÄNDERN", "azubi_group", value: groupId);

        await modal.RespondWithModalAsync(addApprenticeModal.Build());
    }

    public async void SendGroupSelectorDropdown(SocketSlashCommand command, List<Group>? groups, string responseMessage)
    {
        var builder = GroupSelectorComponentBuilder(groups);

        await command.RespondAsync(responseMessage, components: builder.Build());
    }


    public async void SendGroupSelectorDropdown(SocketModal command, List<Group>? groups, string responseMessage)
    {
        var builder = GroupSelectorComponentBuilder(groups);

        await command.RespondAsync(responseMessage, components: builder.Build());
    }

    private static ComponentBuilder GroupSelectorComponentBuilder(List<Group>? groups)
    {
        // Build the group selector menu
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Wähle deine Gruppe aus")
            .WithCustomId("groupSelectorApprentice");

        foreach (var group in groups)
        {
            menuBuilder.AddOption(group.Name, group.Id.ToString());
        }

        menuBuilder.AddOption("Neue Gruppe", "new-group");

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);
        return builder;
    }

    public async void SendApprenticeRemoveConfirmation(SocketSlashCommand command)
    {
        // Build the confirmation button
        var builder = new ComponentBuilder()
            .WithButton("Entfernen", "deleteApprentice", style: ButtonStyle.Danger);

        await command.RespondAsync(
            "Sicher, dass du deinen Account Entfernen möchtest?\nAlle deine Log Einträge werden dabei gelöscht.",
            components: builder.Build());
    }

    // Option ob der aktuell schreibende oder jemand anderes übersprungen werden soll
    public async void SendSkipChoice(SocketSlashCommand command, Apprentice nextBerichtsheftWriter)
    {
        var skipChoiceButtons = new ComponentBuilder()
            .WithButton(nextBerichtsheftWriter.Name, "nextWriterApprentice", style: ButtonStyle.Primary)
            .WithButton("Jemand anderes", "otherApprentice", style: ButtonStyle.Secondary);

        await command.RespondAsync("Wer soll übersprungen werden: ", components: skipChoiceButtons.Build());
    }

    // Selector wer übersprungen werden soll
    public async void SendSkipDropdownChoice(SocketMessageComponent component, List<Apprentice> apprentices)
    {
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Wer soll übersprungen werden")
            .WithCustomId("skipApprenticeSelector");

        foreach (var apprentice in apprentices)
        {
            menuBuilder.AddOption(apprentice.Name, apprentice.Id.ToString());
        }

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);


        await component.RespondAsync("", components: builder.Build());
    }

    // Selector wer übersprungen werden soll
    public async void SendUnSkipDropdownChoice(SocketSlashCommand command, List<Apprentice> apprentices)
    {
        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Wer soll nicht mehr übersprungen werden")
            .WithCustomId("unSkipApprenticeSelector");

        foreach (var apprentice in apprentices)
        {
            menuBuilder.AddOption(apprentice.Name, apprentice.Id.ToString());
        }

        var builder = new ComponentBuilder()
            .WithSelectMenu(menuBuilder);


        await command.RespondAsync("", components: builder.Build());
    }
}