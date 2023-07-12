using BerichtBotNet.Data;
using BerichtBotNet.Discord.View;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public class ApprenticeController
{
    private readonly ApprenticeRepository _apprenticeRepository;
    private readonly GroupRepository _groupRepository;

    private readonly ApprenticeView _apprenticeView;
    private readonly GroupView _groupView;

    public ApprenticeController(ApprenticeRepository apprenticeRepository, GroupRepository groupRepository)
    {
        _apprenticeRepository = apprenticeRepository;
        _groupRepository = groupRepository;

        _apprenticeView = new ApprenticeView();
        _groupView = new GroupView();

    }

    /// <summary>
    /// Handles the slash commands related to apprentices.
    /// </summary>
    /// <param name="command">The SocketSlashCommand object representing the command.</param>
    public void ApprenticeCommandHandler(SocketSlashCommand command)
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
                _apprenticeView.SendApprenticeRemoveConfirmation(command);
                break;
        }
    }

    /// <summary>
    /// Handles the modals related to apprentices.
    /// </summary>
    /// <param name="modal">The SocketModal object representing the modal.</param>
    public async void ApprenticeModalHandler(SocketModal modal)
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
    public async void ApprenticeMessageComponentHandler(SocketMessageComponent component)
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


    private async Task AddApprentice(SocketModal modal)
    {
        // Extract component values from the modal
        List<SocketMessageComponentData> components = modal.Data.Components.ToList();
        string username = components.First(x => x.CustomId == "azubi_name").Value;
        string discordId = components.First(x => x.CustomId == "azubi_id").Value;
        string group = components.First(x => x.CustomId == "azubi_group").Value;

        // Create Apprentice entity
        Apprentice apprentice = new Apprentice
        {
            Name = username,
            DiscordUserId = discordId,
            Group = _groupRepository.GetGroup(int.Parse(group)),
            Skipped = false
        };

        // Save Apprentice to the database
        _apprenticeRepository.CreateApprentice(apprentice);

        // Respond to the user
        string answer = $"Azubi: {username} wurde hinzugefügt.";
        await modal.RespondAsync(answer);
    }

    private async void EditApprentice(SocketModal modal)
    {
        // Extract component values from the modal
        List<SocketMessageComponentData> components = modal.Data.Components.ToList();
        string username = components.First(x => x.CustomId == "azubi_name").Value;
        string discordId = components.First(x => x.CustomId == "azubi_id").Value;
        string group = components.First(x => x.CustomId == "azubi_group").Value;
        string skip = components.First(x => x.CustomId == "azubi_skip").Value;

        // Check if the selected group exists
        if (_groupRepository.GetGroupByName(group) == null)
        {
            await modal.RespondAsync($"Gewählte Gruppe ({group}) konnte nicht gefunden werden.");
            return;
        }

        // Retrieve the Apprentice from the database
        Apprentice? apprentice = _apprenticeRepository.GetApprenticeByDiscordId(modal.User.Id.ToString());

        if (apprentice == null)
        {
            await modal.RespondAsync("Benutzer konnte nicht gefunden werden.");
            return;
        }

        // Update the Apprentice with the new values
        apprentice.Name = username;
        apprentice.DiscordUserId = discordId;
        apprentice.Group = _groupRepository.GetGroupByName(group);
        apprentice.Skipped = int.Parse(skip) == 1;

        // Save the updated Apprentice to the database
        _apprenticeRepository.UpdateApprentice(apprentice);

        // Respond to the user
        string answer = $"Azubi: {username} wurde aktualisiert.";
        await modal.RespondAsync(answer);
    }

    private async void SendEditApprentice(SocketSlashCommand command)
    {
        // Get the Apprentice based on the Discord ID
        Apprentice? user = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        _apprenticeView.SendEditApprenticeModal(command, user);
    }

    private async void GroupSelector(SocketMessageComponent component)
    {
        if (component.Data.Values.First().Equals("new-group"))
        {
            // Add a new group
            _groupView.AddGroup(component);
        }
        else
        {
            // Send the add apprentice modal with the selected group
            _apprenticeView.SendAddApprentice(component, component.Data.Values.First());
        }
    }

    private async void SendGroupSelector(SocketSlashCommand command)
    {
        List<Group>? groups = _groupRepository.GetAllGroups();
        string response = "Wähle deine Gruppe";
        _apprenticeView.SendGroupSelectorDropdown(command, groups, response);
    }

    public async void SendGroupSelector(SocketModal modal, string groupName)
    {
        List<Group>? groups = _groupRepository.GetAllGroups();

        string response = $"Gruppe: {groupName} wurde hinzugefügt\n\n\nWähle jetzt deine Gruppe aus.";
        _apprenticeView.SendGroupSelectorDropdown(modal, groups, response);
    }

    

    private async void RemoveApprentice(SocketMessageComponent component)
    {
        // Get the Apprentice based on the Discord ID
        Apprentice? apprentice = _apprenticeRepository.GetApprenticeByDiscordId(component.User.Id.ToString());

        if (apprentice is not null)
        {
            // Delete the Apprentice from the database
            _apprenticeRepository.DeleteApprentice(apprentice.Id);
            await component.RespondAsync($"Der Account {apprentice.Name} wurde entfernet.");
        }
        else
        {
            await component.RespondAsync("Dein Account konnte nicht gefunden werden.");
        }
    }
}