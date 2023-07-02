using BerichtBotNet.Data;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public class ApprenticeCommands
{
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
        List<SocketMessageComponentData> components =
            modal.Data.Components.ToList();

        string username = components.First(x => x.CustomId == "azubi_name").Value;
        string discordId = components.First(x => x.CustomId == "azubi_id").Value;
        string group = components.First(x => x.CustomId == "azubi_group").Value;


        using BerichtBotContext context = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(context);


        // Creates Apprentice
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        Apprentice apprentice = new Apprentice
        {
            Name = username,
            DiscordUserId = discordId,
            Group = groupRepository.GetGroup(int.Parse(group)),
            SkipCount = 0
        };

        apprenticeRepository.CreateApprentice(apprentice);


        // Answer to User
        string answer = $"Azubi: {username} wurde hinzugefügt.";

        await modal.RespondAsync(answer);
    }

    private static async void EditApprentice(SocketModal modal)
    {
        List<SocketMessageComponentData> components =
            modal.Data.Components.ToList();

        string username = components.First(x => x.CustomId == "azubi_name").Value;
        string discordId = components.First(x => x.CustomId == "azubi_id").Value;
        string group = components.First(x => x.CustomId == "azubi_group").Value;
        string skip = components.First(x => x.CustomId == "azubi_skip").Value;
        bool groupExists = true;


        using BerichtBotContext context = new BerichtBotContext();

        // Creates Group if it doesnt exist
        GroupRepository groupRepository = new GroupRepository(context);
        if (groupRepository.GetGroupByName(group) == null)
        {
            await modal.RespondAsync("Gewählte Gruppe konnte nicht gefunden werden.");
            return;
        }


        // Creates Apprentice
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        Apprentice apprentice = apprenticeRepository.GetApprenticeByDiscordId(modal.User.Id.ToString());

        if (apprentice == null)
        {
            await modal.RespondAsync("Benutzer konnte nicht gefunden werden.");
            return;
        }

        apprentice.Name = username;
        apprentice.DiscordUserId = discordId;
        apprentice.Group = groupRepository.GetGroupByName(group);
        apprentice.SkipCount = int.Parse(skip);

        apprenticeRepository.UpdateApprentice(apprentice);


        // Answer to User
        string answer = $"Azubi: {username} wurde aktualisiert.";
        if (!groupExists)
        {
            answer += $" Die Gruppe {group} Existierte noch nicht und wurde erstellt.";
        }

        await modal.RespondAsync(answer);
    }

    private static async void SendEditApprentice(SocketSlashCommand command)
    {
        using BerichtBotContext context = new BerichtBotContext();
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        Apprentice? user = apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());

        if (user is not null)
        {
            var addApprenticeModal = new ModalBuilder()
                .WithTitle("Azubi Bearbeiten")
                .WithCustomId("editApprenticeMenu")
                .AddTextInput("Name", "azubi_name", value: user.Name)
                .AddTextInput("Discord Id", "azubi_id", value: user.DiscordUserId)
                .AddTextInput("Gruppe", "azubi_group", value: user.Group?.Name)
                .AddTextInput("Überspringen", "azubi_skip", value: user.SkipCount.ToString(), placeholder: "Anzahl, die Azubi übersprungen werden soll. -1 für Unendlich");

            await command.RespondWithModalAsync(addApprenticeModal.Build());
        }
        else
        {
            await command.RespondAsync("Benutzer konnte nicht gefunden werden.");
        }
    }

    // Add Apprentice from a SlashCommand
    private static async void SendAddApprentice(SocketSlashCommand command)
    {
        var addApprenticeModal = new ModalBuilder()
            .WithTitle("Azubi Hinzufügen")
            .WithCustomId("addApprenticeMenu")
            .AddTextInput("Name", "azubi_name", value: command.User.Username)
            .AddTextInput("Discord Id", "azubi_id", value: command.User.Id.ToString());

        await command.RespondWithModalAsync(addApprenticeModal.Build());
    }
    
    // Add Apprentice from a Modal
    public static async void SendAddApprentice(SocketMessageComponent modal, string groupId)
    {
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
            GroupCommands.AddGroup(component);
        }
        else
        {
            SendAddApprentice(component, component.Data.Values.First());
        }
    }

    private static async void SendGroupSelector(SocketSlashCommand command)
    {
        using BerichtBotContext context = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(context);

        List<Group>? groups = groupRepository.GetAllGroups();

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

        var menuBuilder = new SelectMenuBuilder()
            .WithPlaceholder("Wähle deine Gruppe aus")
            .WithCustomId("groupSelectorApprentice")
            .WithMinValues(1)
            .WithMaxValues(1);

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
        var builder = new ComponentBuilder()
            .WithButton("Entfernen", "deleteApprentice", style: ButtonStyle.Danger);

        await command.RespondAsync("Sicher, dass du deinen Account Entfernen möchtest?", components: builder.Build());
    }

    private static async void RemoveApprentice(SocketMessageComponent component)
    {
        using BerichtBotContext context = new BerichtBotContext();
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        Apprentice? apprentice = apprenticeRepository.GetApprenticeByDiscordId(component.User.Id.ToString());

        if (apprentice is not null)
        {
            apprenticeRepository.DeleteApprentice(apprentice.Id);
            await component.RespondAsync($"Der Account {apprentice.Name} wurde entfernet.");
        }
        else
        {
            await component.RespondAsync("Dein Account konnte nicht gefunden werden.");
        }
    }
}