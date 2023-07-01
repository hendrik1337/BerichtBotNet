using BerichtBotNet.Data;
using BerichtBotNet.Helper;
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
    
    private static async Task AddGroupAndApprentice(SocketModal modal)
    {
        using BerichtBotContext context = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(context);
        
        var groupName = AddGroupFromModal(modal, out var group);

        if (groupRepository.GetGroupByName(groupName) == null)
        {
            groupRepository.CreateGroup(group);
            ApprenticeCommands.SendGroupSelector(modal);
        }
        else
        {
            await modal.RespondAsync($"Gruppe: {group.Name} existiert bereits");
        }
    }

    private static string AddGroupFromModal(SocketModal modal, out Group group)
    {
        List<SocketMessageComponentData> components =
            modal.Data.Components.ToList();

        string groupName = components.First(x => x.CustomId == "group_name").Value;
        string groupId = components.First(x => x.CustomId == "group_id").Value;
        string groupStart = components.First(x => x.CustomId == "group_start").Value;

        DateTime dateTimeGroupStart = DateTime.Parse(groupStart, Misc.CultureInfo).ToUniversalTime();

        group = new Group()
        {
            Name = groupName,
            StartOfApprenticeship = dateTimeGroupStart
        };
        return groupName;
    }

    private static ModalBuilder GetAddGroupModal()
    {
        return new ModalBuilder()
            .WithTitle("Gruppe Hinzufügen")
            .AddTextInput("Name", "group_name", placeholder: "FI-22")
            .AddTextInput("Ausbildungsstart", "group_start", placeholder:"03.08.2022")
            .AddTextInput("Discord Id", "group_id", placeholder: "Ignore Me", required: false);
    }

    // Adds group via direct slash command
    private static async void AddGroup(SocketSlashCommand command)
    {
        var addApprenticeModal = GetAddGroupModal();
        addApprenticeModal.WithCustomId("addGroupMenu");
        await command.RespondWithModalAsync(addApprenticeModal.Build());
    }
    
    // Adds group via MessageComponent
    public static async void AddGroup(SocketMessageComponent component)
    {
        var addApprenticeModal = GetAddGroupModal();
        addApprenticeModal.WithCustomId("addGroupMenuAndApprentice");
        await component.RespondWithModalAsync(addApprenticeModal.Build());
    }
}