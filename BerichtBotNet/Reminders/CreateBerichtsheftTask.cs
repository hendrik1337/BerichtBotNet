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
        string ausbildungsjahr = WeekHelper.GetAusbildungsjahr(group.StartOfApprenticeship);

        string response =
            await BerichtsheftService.GenerateBerichtsheft(berichtsheftNumber.ToString(), group.Name, ausbildungsjahr);
        Console.WriteLine(response);
        await SendReminder(group, client, berichtsheftNumber.ToString());
    }

    private async Task SendReminder(Group group, DiscordSocketClient client, string berichtsheftNumber)
    {
        string berichtsheftServerUrl = Environment.GetEnvironmentVariable("berichtsheftServerUrl");
        var channel = await client.GetChannelAsync(ulong.Parse(group.DiscordGroupId)) as IMessageChannel;
        await channel!.SendMessageAsync(
            $"Das Berichtsheft Nr: {berichtsheftNumber} wurde erstellt und ist verfügbar unter:" +
            $" {berichtsheftServerUrl} ");
    }
}