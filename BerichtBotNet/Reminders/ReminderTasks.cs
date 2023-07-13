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

        ApprenticeRepository apprenticeRepository = (ApprenticeRepository)(dataMap)["apprenticeRepository"];
        LogRepository logRepository = (LogRepository)(dataMap)["logRepository"];
        SkippedWeeksRepository weeksRepository = (SkippedWeeksRepository)(dataMap)["weeksRepository"];

        Berichtsheft berichtsheft = new Berichtsheft(apprenticeRepository, logRepository, weeksRepository);

        var group = groupRepository.GetGroup(groupId);
        if (group is null) return;

        Console.WriteLine($"Sending Reminder for group {group.Name}");
        await SendReminder(group, client, berichtsheft);
    }

    private async Task SendReminder(Group group, DiscordSocketClient client, Berichtsheft berichtsheft)
    {
        var channel = await client.GetChannelAsync(ulong.Parse(group.DiscordGroupId)) as IMessageChannel;
        await channel!.SendMessageAsync(berichtsheft.CurrentBerichtsheftWriterMessage(group, true));
    }
}