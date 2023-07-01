// See https://aka.ms/new-console-template for more information

using BerichtBotNet.Discord;
using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Reminders;
using BerichtBotNet.Repositories;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace BerichtBotNet;

class BerichtBotNet
{
    private DiscordSocketClient _client = null!;

    public static Task Main(string[] args) => new BerichtBotNet().MainAsync();

    public async Task MainAsync()
    {

        _client = new DiscordSocketClient();

        _client.Log += Log;

        _client.SlashCommandExecuted += SlashCommandHandler;

        _client.Ready += Client_Ready;

        _client.ModalSubmitted += ModalSubmittedHandler;

        _client.ButtonExecuted += MyMessageComponentHandler;
        
        _client.SelectMenuExecuted += MyMessageComponentHandler;

        var token = Environment.GetEnvironmentVariable("DiscordToken");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await LoadTaskScheduler();

        // Block this task until the program is closed.
        // await Task.Delay(-1);
    }

    private static async Task LoadTaskScheduler()
    {
        // Load the Task Scheduler
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((cxt, services) =>
            {
                services.AddQuartz(q => { q.UseMicrosoftDependencyInjectionJobFactory(); });
                services.AddQuartzHostedService(opt => { opt.WaitForJobsToComplete = true; });
            }).Build();

        var schedulerFactory = builder.Services.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();

        // Define the job and tie it to our ReminderTasks class
        var job = JobBuilder.Create<ReminderTasks>()
            .WithIdentity("myJob", "group1")
            .Build();
        
        var trigger = TriggerBuilder.Create()
            .WithIdentity("myTrigger", "group1")
            .StartNow()
            .WithSchedule(CronScheduleBuilder
                .WeeklyOnDayAndHourAndMinute(DayOfWeek.Monday, 8, 30))
            .Build();

        await scheduler.ScheduleJob(job, trigger);
        
        await builder.RunAsync();
    }

    private async Task Client_Ready()
    {
        /*
        var globalAzubiCommand = new SlashCommandBuilder()
            .WithName("wer")
            .WithDescription("Gibt den Berichsheftschreiber zurück")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("nummer")
                .WithDescription("Welche Berichsheftnummer? (optional)")
                .WithRequired(false)
                .AddChoice("nummer", "number")
                .WithType(ApplicationCommandOptionType.String)
            );

        try
        {
            await _client.Rest.CreateGlobalCommand(globalAzubiCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }*/
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "azubi":
                ApprenticeCommands.ApprenticeCommandHandler(command);
                break;
            case "gruppe":
                GroupCommands.GroupCommandHandler(command);
                break;
            case "wer":
                BerichtsheftCommands.BerichtsheftCommandHandler(command);
                break;
        }
    }

    private async Task ModalSubmittedHandler(SocketModal modal)
    {
        if (modal.Data.CustomId.Contains("ApprenticeMenu"))
        {
            ApprenticeCommands.ApprenticeModalHandler(modal);
        }

        if (modal.Data.CustomId.Contains("GroupMenu"))
        {
            GroupCommands.GroupModalHandler(modal);
        }
    }
    
    public async Task MyMessageComponentHandler(SocketMessageComponent component)
    {
        if (component.Data.CustomId.Contains("Apprentice"))
        {
            ApprenticeCommands.ApprenticeMessageComponentHandler(component);
        }
    }


    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}