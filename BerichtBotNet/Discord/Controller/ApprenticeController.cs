using BerichtBotNet.Data;
using BerichtBotNet.Discord.View;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Helper;
using BerichtBotNet.Repositories;
using Discord.WebSocket;

namespace BerichtBotNet.Discord.Controller;

public class ApprenticeController
{
    private readonly ApprenticeRepository _apprenticeRepository;
    private readonly GroupRepository _groupRepository;
    private readonly LogRepository _logRepository;
    private readonly SkippedWeeksRepository _weeksRepository;
    private Berichtsheft _berichtsheft;

    private readonly ApprenticeView _apprenticeView;
    private readonly GroupView _groupView;

    public ApprenticeController(ApprenticeRepository apprenticeRepository, GroupRepository groupRepository,
        LogRepository logRepository, SkippedWeeksRepository weeksRepository)
    {
        _apprenticeRepository = apprenticeRepository;
        _groupRepository = groupRepository;
        _logRepository = logRepository;
        _weeksRepository = weeksRepository;

        _apprenticeView = new ApprenticeView();
        _groupView = new GroupView();

        _berichtsheft = new Berichtsheft(_apprenticeRepository, _logRepository, _weeksRepository);
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
            case "skip":
                SkipApprentice(command);
                break;
            case "un-skip":
                UnSkipApprentice(command);
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
            case "nextWriterApprentice":
                SkipNextWriter(component);
                break;
            case "otherApprentice":
                ChooseApprenticeToSkip(component);
                break;
            case "skipApprenticeSelector":
                SetSkipOfSpecificApprentice(component, true);
                break;
            case "unSkipApprenticeSelector":
                SetSkipOfSpecificApprentice(component, false);
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
    
    private async void UnSkipApprentice(SocketSlashCommand command)
    {
        var requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        if (!ValidateRequest(command, requester).Result) return;


        var apprentices = _apprenticeRepository.GetApprenticesInSameGroupByGroupId(requester.Group.Id);
        List<Apprentice> skippedApprentices = _berichtsheft.FilterApprenticesBySkipCount(apprentices, true);

        if (skippedApprentices.Count == 0)
        {
            await command.RespondAsync("Es wird kein Azubi übersprungen");
            return;
        }

        _apprenticeView.SendUnSkipDropdownChoice(command, skippedApprentices);
    }

    private async void SetSkipOfSpecificApprentice(SocketMessageComponent component, bool skipped)
    {
        var requester = _apprenticeRepository.GetApprenticeByDiscordId(component.User.Id.ToString());
        if (!ValidateRequest(component, requester).Result) return;

        Apprentice currentBerichtsheftWriter = _berichtsheft.GetCurrentBerichtsheftWriterOfGroup(requester.Group.Id);

        var apprenticeId = string.Join(", ", component.Data.Values);
        var apprenticeToSkip = _apprenticeRepository.GetApprentice(int.Parse(apprenticeId));
        apprenticeToSkip.Skipped = skipped;
        _apprenticeRepository.UpdateApprentice(apprenticeToSkip);

        if (skipped)
        {
            await component.RespondAsync($"Azubi: {apprenticeToSkip.Name} wird übersprungen.");
        }
        else
        {
            await component.RespondAsync($"Azubi: {apprenticeToSkip.Name} wird nicht mehr übersprungen.");
        }

        string ans;

        if (currentBerichtsheftWriter.Id == _berichtsheft.GetCurrentBerichtsheftWriterOfGroup(requester.Group.Id).Id)
        {
            ans = _berichtsheft.CurrentBerichtsheftWriterMessage(apprenticeToSkip.Group, false);
        }
        else
        {
            ans = _berichtsheft.CurrentBerichtsheftWriterMessage(apprenticeToSkip.Group, true);
        }

        
        await component.Channel.SendMessageAsync(ans);
    }

    private async void ChooseApprenticeToSkip(SocketMessageComponent component)
    {
        var requester = _apprenticeRepository.GetApprenticeByDiscordId(component.User.Id.ToString());
        if (!ValidateRequest(component, requester).Result) return;

        var apprentices = _apprenticeRepository.GetApprenticesInSameGroupByGroupId(requester.Group.Id);
        var nonSkippedApprentices = _berichtsheft.FilterApprenticesBySkipCount(apprentices, false);

        if (nonSkippedApprentices.Count == 0)
        {
            await component.RespondAsync("Es gibt keinen Azubi zum überspringen");
            return;
        }

        _apprenticeView.SendSkipDropdownChoice(component, nonSkippedApprentices);
    }

    private async void SkipNextWriter(SocketMessageComponent component)
    {
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(component.User.Id.ToString());
        if (!ValidateRequest(component, requester).Result) return;

        Apprentice? nextBerichtsheftWriter = _berichtsheft.GetCurrentBerichtsheftWriterOfGroup(requester.Group.Id);
        nextBerichtsheftWriter.Skipped = true;
        _apprenticeRepository.UpdateApprentice(nextBerichtsheftWriter);

        await component.RespondAsync($"Azubi: {nextBerichtsheftWriter.Name} wird übersprungen.");

        string ans = _berichtsheft.CurrentBerichtsheftWriterMessage(nextBerichtsheftWriter.Group, true);
        await component.Channel.SendMessageAsync(ans);
    }

    private async Task AddApprentice(SocketModal modal)
    {
        // Extract component values from the modal
        List<SocketMessageComponentData> components = modal.Data.Components.ToList();
        string username = components.First(x => x.CustomId == "azubi_name").Value;
        string discordId = components.First(x => x.CustomId == "azubi_id").Value;
        string groupName = components.First(x => x.CustomId == "azubi_group").Value;

        if (_apprenticeRepository.GetApprenticeByDiscordId(discordId) is not null)
        {
            await modal.RespondAsync("Registrieren Fehlgeschlagen.\nEs gibt bereite einen Nutzer mit dieser Discord ID.");
            return;
        }

        // Create Apprentice entity
        Apprentice apprentice = new Apprentice
        {
            Name = username,
            DiscordUserId = discordId,
            Group = _groupRepository.GetGroupByName(groupName),
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
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(modal.User.Id.ToString());

        if (!ValidateRequest(modal, requester).Result) return;

        // Update the Apprentice with the new values
        requester.Name = username;
        requester.DiscordUserId = discordId;
        requester.Group = _groupRepository.GetGroupByName(group);
        requester.Skipped = int.Parse(skip) == 1;

        // Save the updated Apprentice to the database
        _apprenticeRepository.UpdateApprentice(requester);

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
            Group group = _groupRepository.GetGroup(int.Parse(component.Data.Values.First()));
            _apprenticeView.SendAddApprentice(component, group.Name);
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
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(component.User.Id.ToString());

        if (!ValidateRequest(component, requester).Result) return;

        _apprenticeRepository.DeleteApprentice(requester.Id);
        await component.RespondAsync($"Der Account {requester.Name} wurde entfernet.");
    }

    private async void SkipApprentice(SocketSlashCommand command)
    {
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        if (!ValidateRequest(command, requester).Result) return;

        try
        {
            Apprentice? nextBerichtsheftWriter = _berichtsheft.GetCurrentBerichtsheftWriterOfGroup(requester.Group.Id);
            _apprenticeView.SendSkipChoice(command, nextBerichtsheftWriter);
        }
        catch (GroupIsEmptyException ignored)
        {
            await command.RespondAsync("Es gibt keinen Azubi zum überspringen");
        }
        
    }
}