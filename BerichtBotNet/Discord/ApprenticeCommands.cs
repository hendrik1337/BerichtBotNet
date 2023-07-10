using BerichtBotNet.Data;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public class ApprenticeCommands
{
    /// <summary>
    /// Handles the slash commands related to apprentices.
    /// </summary>
    /// <param name="command">The SocketSlashCommand object representing the command.</param>
    public static void ApprenticeCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Options.First().Value)
        {
            case "add":
                SendGroupSelector(command);
                break;
            case "edit":
                SendEditApprentice(command);
                break;
            case "remove":
                SendApprenticeRemoveConfirmation(command);
                break;
        }
    }

    /// <summary>
    /// Handles the modals related to apprentices.
    /// </summary>
    /// <param name="modal">The SocketModal object representing the modal.</param>
    public static async void ApprenticeModalHandler(SocketModal modal)
    {
        switch (modal.Data.CustomId)
        {
            case "addApprenticeMenu":
                await AddApprentice(modal);
                break;
            case "editApprenticeMenu":
                EditApprentice(modal);
                break;
        }
    }

    /// <summary>
    /// Handles the message components related to apprentices.
    /// </summary>
    /// <param name="component">The SocketMessageComponent object representing the message component.</param>
    public static async void ApprenticeMessageComponentHandler(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "deleteApprentice":
                RemoveApprentice(component);
                break;
            case "groupSelectorApprentice":
                GroupSelector(component);
                break;
        }
    }
    
    

    private static async Task AddApprentice(SocketModal modal)
    {
        // Extract component values from the modal
        List<SocketMessageComponentData> components = modal.Data.Components.ToList();
        string username = components.First(x => x.CustomId == "azubi_name").Value;
        string discordId = components.First(x => x.CustomId == "azubi_id").Value;
        string group = components.First(x => x.CustomId == "azubi_group").Value;

        // Create database context and repositories
        using BerichtBotContext context = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(context);

        // Create Apprentice entity
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        Apprentice apprentice = new Apprentice
        {
            Name = username,
            DiscordUserId = discordId,
            Group = groupRepository.GetGroup(int.Parse(group)),
            SkipCount = 0
        };

        // Save Apprentice to the database
        apprenticeRepository.CreateApprentice(apprentice);

        // Respond to the user
        string answer = $"Azubi: {username} wurde hinzugefügt.";
        await modal.RespondAsync(answer);
    }

    private static async void EditApprentice(SocketModal modal)
    {
        // Extract component values from the modal
        List<SocketMessageComponentData> components = modal.Data.Components.ToList();
        string username = components.First(x => x.CustomId == "azubi_name").Value;
        string discordId = components.First(x => x.CustomId == "azubi_id").Value;
        string group = components.First(x => x.CustomId == "azubi_group").Value;
        string skip = components.First(x => x.CustomId == "azubi_skip").Value;

        // Create database context and repositories
        await using BerichtBotContext context = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(context);

        // Check if the selected group exists
        if (groupRepository.GetGroupByName(group) == null)
        {
            await modal.RespondAsync($"Gewählte Gruppe ({group}) konnte nicht gefunden werden.");
            return;
        }

        // Retrieve the Apprentice from the database
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        Apprentice apprentice = apprenticeRepository.GetApprenticeByDiscordId(modal.User.Id.ToString());

        if (apprentice == null)
        {
            await modal.RespondAsync("Benutzer konnte nicht gefunden werden.");
            return;
        }

        // Update the Apprentice with the new values
        apprentice.Name = username;
        apprentice.DiscordUserId = discordId;
        apprentice.Group = groupRepository.GetGroupByName(group);
        apprentice.SkipCount = int.Parse(skip);

        // Save the updated Apprentice to the database
        apprenticeRepository.UpdateApprentice(apprentice);

        // Respond to the user
        string answer = $"Azubi: {username} wurde aktualisiert.";
        await modal.RespondAsync(answer);
    }

    private static async void SendEditApprentice(SocketSlashCommand command)
{
    using BerichtBotContext context = new BerichtBotContext();
    ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);

    // Get the Apprentice based on the Discord ID
    Apprentice? user = apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());

    if (user is not null)
    {
        // Build and send the edit apprentice modal
        var addApprenticeModal = new ModalBuilder()
            .WithTitle("Azubi Bearbeiten")
            .WithCustomId("editApprenticeMenu")
            .AddTextInput("Name", "azubi_name", value: user.Name)
            .AddTextInput("Discord Id", "azubi_id", value: user.DiscordUserId)
            .AddTextInput("Gruppe", "azubi_group", value: user.Group?.Name, placeholder: "Stelle sicher, dass die Gruppe existiert.")
            .AddTextInput("Überspringen", "azubi_skip", value: user.SkipCount.ToString(), placeholder: "0 = Wird nicht übersprungen, 1 = wird übersprungen");

        await command.RespondWithModalAsync(addApprenticeModal.Build());
    }
    else
    {
        await command.RespondAsync("Benutzer konnte nicht gefunden werden.");
    }
}

private static async void SendAddApprentice(SocketSlashCommand command)
{
    // Build and send the add apprentice modal with default values from the command user
    var addApprenticeModal = new ModalBuilder()
        .WithTitle("Azubi Hinzufügen")
        .WithCustomId("addApprenticeMenu")
        .AddTextInput("Name", "azubi_name", value: command.User.Username)
        .AddTextInput("Discord Id", "azubi_id", value: command.User.Id.ToString());

    await command.RespondWithModalAsync(addApprenticeModal.Build());
}

public static async void SendAddApprentice(SocketMessageComponent modal, string groupId)
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

private static async void GroupSelector(SocketMessageComponent component)
{
    if (component.Data.Values.First().Equals("new-group"))
    {
        // Add a new group
        GroupCommands.AddGroup(component);
    }
    else
    {
        // Send the add apprentice modal with the selected group
        SendAddApprentice(component, component.Data.Values.First());
    }
}

private static async void SendGroupSelector(SocketSlashCommand command)
{
    using BerichtBotContext context = new BerichtBotContext();
    GroupRepository groupRepository = new GroupRepository(context);

    List<Group>? groups = groupRepository.GetAllGroups();

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

    await command.RespondAsync("Wähle deine Gruppe", components: builder.Build());
}

public static async void SendGroupSelector(SocketModal modal, string groupName)
{
    using BerichtBotContext context = new BerichtBotContext();
    GroupRepository groupRepository = new GroupRepository(context);

    List<Group>? groups = groupRepository.GetAllGroups();

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

    await modal.RespondAsync($"Gruppe: {groupName} wurde hinzugefügt\n\n\nWähle jetzt deine Gruppe aus.", components: builder.Build());
}

private static async void SendApprenticeRemoveConfirmation(SocketSlashCommand command)
{
    // Build the confirmation button
    var builder = new ComponentBuilder()
        .WithButton("Entfernen", "deleteApprentice", style: ButtonStyle.Danger);

    await command.RespondAsync("Sicher, dass du deinen Account Entfernen möchtest?", components: builder.Build());
}

private static async void RemoveApprentice(SocketMessageComponent component)
{
    using BerichtBotContext context = new BerichtBotContext();
    ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);

    // Get the Apprentice based on the Discord ID
    Apprentice? apprentice = apprenticeRepository.GetApprenticeByDiscordId(component.User.Id.ToString());

    if (apprentice is not null)
    {
        // Delete the Apprentice from the database
        apprenticeRepository.DeleteApprentice(apprentice.Id);
        await component.RespondAsync($"Der Account {apprentice.Name} wurde entfernet.");
    }
    else
    {
        await component.RespondAsync("Dein Account konnte nicht gefunden werden.");
    }
}
}