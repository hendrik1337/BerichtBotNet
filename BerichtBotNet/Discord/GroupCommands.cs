using BerichtBotNet.Data;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;

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
            case "addApprenticeMenu":
                await AddGroup(modal);
                break;
        }
    }

    private static async Task AddGroup(SocketModal modal)
    {
        List<SocketMessageComponentData> components =
            modal.Data.Components.ToList();

        string groupName = components.First(x => x.CustomId == "group_name").Value;
        string groupId = components.First(x => x.CustomId == "group_id").Value;


        using BerichtBotContext context = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(context);


        Group group = new Group()
        {
            Name = groupName,
        };

        if (groupRepository.GetGroupByName(groupName) == null)
        {
            groupRepository.CreateGroup(group);
            await modal.RespondAsync($"Gruppe: {group} wurde hinzugefügt");
        }
        else
        {
            await modal.RespondAsync($"Gruppe: {group} existiert bereits");
        }

        
    }

    private static async void AddGroup(SocketSlashCommand command)
    {
        var addApprenticeModal = new ModalBuilder()
            .WithTitle("Gruppe Hinzufügen")
            .WithCustomId("addGroupMenu")
            .AddTextInput("Name", "group_name", placeholder: "FI-22")
            .AddTextInput("Discord Id", "group_id", placeholder: "1337", required: false);

        await command.RespondWithModalAsync(addApprenticeModal.Build());
    }
}