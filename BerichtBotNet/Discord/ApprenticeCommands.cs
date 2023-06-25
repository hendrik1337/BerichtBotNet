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
                AddApprentice(command);
                break;
            case "edit":
                break;
            case "remove":
                break;
        }
    }

    public static async void ApprenticeModalHandler(SocketModal modal)
    {
        switch (modal.Data.CustomId)
        {
            case "addApprenticeMenu":
                await addApprentice(modal);
                break;
        }
    }

    private static async Task addApprentice(SocketModal modal)
    {
        List<SocketMessageComponentData> components =
            modal.Data.Components.ToList();

        string username = components.First(x => x.CustomId == "azubi_name").Value;
        string discordId = components.First(x => x.CustomId == "azubi_id").Value;
        string group = components.First(x => x.CustomId == "azubi_group").Value;
        bool groupExists = true;


        using BerichtBotContext context = new BerichtBotContext();

        // Creates Group if it doesnt exist
        GroupRepository groupRepository = new GroupRepository(context);
        
        if (groupRepository.GetGroupByName(group) == null)
        {
            groupRepository.CreateGroup(new Group(){Name = group});
            groupExists = false;
        }


        // Creates Apprentice
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        Apprentice apprentice = new Apprentice
        {
            Name = username,
            DiscordUserId = discordId,
            Group = groupRepository.GetGroupByName(group),
            SkipCount = 0
        };

        apprenticeRepository.CreateApprentice(apprentice);
        
        
        // Answer to User
        string answer = $"Azubi: {username} wurde hinzugefügt.";
        if (!groupExists)
        {
            answer += $" Die Gruppe {group} Existierte noch nicht und wurde erstellt.";
        }
        await modal.RespondAsync(answer);
    }

    private static async void AddApprentice(SocketSlashCommand command)
    {
        var addApprenticeModal = new ModalBuilder()
            .WithTitle("Azubi Hinzufügen")
            .WithCustomId("addApprenticeMenu")
            .AddTextInput("Name", "azubi_name", value: command.User.Username)
            .AddTextInput("Discord Id", "azubi_id", value: command.User.Id.ToString())
            .AddTextInput("Gruppe", "azubi_group", placeholder: "FI-22");

        await command.RespondWithModalAsync(addApprenticeModal.Build());
    }
}