using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace BerichtBotNet.Reminders;

public class ReminderTasks : IJob
{
    // TODO Woche muss überspringbar sein
    public async Task Execute(IJobExecutionContext context)
    {
        
        // Your code here
        // This function will be executed at custom time

        await using BerichtBotContext dataContext = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(dataContext);

        var dataMap = context.MergedJobDataMap;
        int groupId = (int)(dataMap)["groupId"];
        var client = (DiscordSocketClient)dataMap["discord"];

        var group = groupRepository.GetGroup(groupId);
        Console.WriteLine($"Sending Reminder for group {group.Name}");
        await SendReminder(group.DiscordGroupId, client);
    }

    private async Task SendReminder(string groupId, DiscordSocketClient client)
    {
        var channel = await client.GetChannelAsync(ulong.Parse(groupId)) as IMessageChannel;
        await channel!.SendMessageAsync("Hello world");
    }
}