using BerichtBotNet.Data;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;
using Quartz;

namespace BerichtBotNet.Reminders;
/*
public class CreateBerichtsheftTask : IJob
{
    
    public Task Execute(IJobExecutionContext context)
    {
        // This function will be executed every Saturday at 08:00
        var dataMap = context.MergedJobDataMap;
        
        ApprenticeRepository apprenticeRepository = (ApprenticeRepository)(dataMap)["apprenticeRepository"];
        Apprentice apprentice = apprenticeRepository.GetApprenticeByDiscordId("757579143490568194");
        
        
        
    }
    
    private async Task SendReminder(Group group, DiscordSocketClient client)
    {
        string berichtsheftServerUrl = Environment.GetEnvironmentVariable("berichtsheftServerUrl");
        var channel = await client.GetChannelAsync(ulong.Parse(group.DiscordGroupId)) as IMessageChannel;
        await channel!.SendMessageAsync($"Das Berichtsheft wurde erstellt und ist verfügbar unter:" +
                                        $" {berichtsheftServerUrl} ");
    }
}
*/