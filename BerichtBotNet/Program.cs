// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using BerichtBotNet.Data;
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
    private IScheduler _scheduler = null!;
    private CancellationTokenSource _cancellationTokenSource;

    public static Task Main(string[] args) => new BerichtBotNet().MainAsync();

    public async Task MainAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();

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
        /*try
        {
            await Task.Delay(-1, _cancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Task was canceled, perform cleanup here
            await _scheduler.Shutdown();
            await _client.StopAsync();
        }*/
    }

    private async Task LoadTaskScheduler()
    {
        using BerichtBotContext context = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(context);
        List<Group> groups = groupRepository.GetAllGroups();

        // Load the Task Scheduler
        var builder = Host.CreateDefaultBuilder()
            .ConfigureServices((cxt, services) =>
            {
                services.AddQuartz(q => { q.UseMicrosoftDependencyInjectionJobFactory(); });
                services.AddQuartzHostedService(opt => { opt.WaitForJobsToComplete = true; });
            }).Build();

        var schedulerFactory = builder.Services.GetRequiredService<ISchedulerFactory>();
        _scheduler = await schedulerFactory.GetScheduler();

        // Create Custom Reminder Job for every group
        foreach (var group in groups)
        {
            // Define the job and tie it to our ReminderTasks class
            var reminderJob = JobBuilder.Create<ReminderTasks>()
                .WithIdentity($"myGroupReminderJob{group.Id.ToString()}", $"groupReminder{group.Id.ToString()}")
                .Build();

            reminderJob.JobDataMap.Put("discord", _client);
            reminderJob.JobDataMap.Put("groupId", group.Id);

            var reminderTrigger = TriggerBuilder.Create()
                .WithIdentity($"myGroupReminderTrigger{group.Id.ToString()}", $"groupReminder{group.Id.ToString()}")
                .StartNow()
                .WithSchedule(CronScheduleBuilder
                    .WeeklyOnDayAndHourAndMinute(
                        group.ReminderWeekDay,
                        group.ReminderTime.ToLocalTime().Hour,
                        group.ReminderTime.ToLocalTime().Minute))
                .Build();

            await _scheduler.ScheduleJob(reminderJob, reminderTrigger);
        }


        var updateApprentices = JobBuilder.Create<UpdateCurrentApprenticeTask>()
            .WithIdentity("updateApprentices", "group2")
            .Build();


        var updateApprenticesTrigger = TriggerBuilder.Create()
            .WithIdentity("myTrigger2", "group2")
            .StartNow()
            .WithSchedule(CronScheduleBuilder
                .WeeklyOnDayAndHourAndMinute(DayOfWeek.Sunday, 20, 0))
            .Build();


        await _scheduler.ScheduleJob(updateApprentices, updateApprenticesTrigger);
        await _scheduler.Start();

        await builder.RunAsync();
    }

    private async Task Client_Ready()
    {
        await CommandCreator.CreateCommands(_client);
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
            case "woche":
                WeekCommands.WeekCommandHandler(command);
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

    public async Task StopBot()
    {
        _cancellationTokenSource.Cancel();
        await _scheduler.Shutdown();
        await _client.StopAsync();
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}