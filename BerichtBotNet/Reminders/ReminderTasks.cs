using BerichtBotNet.Data;
using BerichtBotNet.Exceptions;
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

        var dataMap = context.MergedJobDataMap;
        int groupId = (int)(dataMap)["groupId"];
        GroupRepository groupRepository = (GroupRepository)(dataMap)["groupRepository"];
        var client = (DiscordSocketClient)dataMap["discord"];

        BerichtsheftService berichtsheftService = (BerichtsheftService)(dataMap)["berichtsheftService"];

        var group = groupRepository.GetGroup(groupId);
        if (group is null) return;

        Console.WriteLine($"Sending Reminder for group {group.Name}");
        await SendReminder(group, client, berichtsheftService);
    }

    private async Task SendReminder(Group group, DiscordSocketClient client, BerichtsheftService berichtsheftService)
    {
        var channel = await client.GetChannelAsync(ulong.Parse(group.DiscordGroupId)) as IMessageChannel;
        await channel!.SendMessageAsync(berichtsheftService.CurrentBerichtsheftWriterMessage(group, true));
    }
}