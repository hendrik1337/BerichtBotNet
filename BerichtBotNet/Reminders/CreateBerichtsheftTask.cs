using BerichtBotNet.Data;
using BerichtBotNet.Helper;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace BerichtBotNet.Reminders;

public class CreateBerichtsheftTask : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // This function will be executed every Saturday at 08:00
        var dataMap = context.MergedJobDataMap;

        var client = (DiscordSocketClient)dataMap["discord"];
        var group = (Group)dataMap["group"];
        
        int berichtsheftNumber = WeekHelper.GetBerichtsheftNumber(group.StartOfApprenticeship, DateTime.Now);

        string response =
            await BerichtsheftService.GenerateBerichtsheft(berichtsheftNumber.ToString(), group.Name);
        Console.WriteLine(response);
        await SendReminder(group, client);
    }

    private async Task SendReminder(Group group, DiscordSocketClient client)
    {
        string berichtsheftServerUrl = Environment.GetEnvironmentVariable("berichtsheftServerUrl");
        var channel = await client.GetChannelAsync(ulong.Parse(group.DiscordGroupId)) as IMessageChannel;
        await channel!.SendMessageAsync($"Das Berichtsheft wurde erstellt und ist verfügbar unter:" +
                                        $" {berichtsheftServerUrl} ");
    }
}