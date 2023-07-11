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

    private ApprenticeRepository _apprenticeRepository;
    private GroupRepository _groupRepository;
    private LogRepository _logRepository;
    private SkippedWeeksRepository _weeksRepository;
    
    private BerichtsheftController _berichtsheftController;
    private ApprenticeController _apprenticeController;
    private GroupController _groupController;
    private WeekController _weekController;

    public static Task Main(string[] args) => new BerichtBotNet().MainAsync();

    public async Task MainAsync()
    {
        InitializeCommandHandlers();
        
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

    private void InitializeCommandHandlers()
    {
        BerichtBotContext context = new BerichtBotContext();
        _apprenticeRepository = new ApprenticeRepository(context);
        _groupRepository = new GroupRepository(context);
        _logRepository = new LogRepository(context);
        _weeksRepository = new SkippedWeeksRepository(context);

        _apprenticeController = new ApprenticeController(_apprenticeRepository, _groupRepository);
        _berichtsheftController = new BerichtsheftController(_apprenticeRepository, _weeksRepository, _logRepository);
        _groupController = new GroupController(_groupRepository);
        _weekController = new WeekController(_apprenticeRepository, _weeksRepository);
    }

    private async Task LoadTaskScheduler()
    {
        List<Group> groups = _groupRepository.GetAllGroups();

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
            Console.WriteLine($"Reminder Scheduled for {group.ReminderTime.ToLocalTime()}");
            // Define the job and tie it to our ReminderTasks class
            var reminderJob = JobBuilder.Create<ReminderTasks>()
                .WithIdentity($"myGroupReminderJob{group.Id.ToString()}", $"groupReminder{group.Id.ToString()}")
                .Build();

            reminderJob.JobDataMap.Put("discord", _client);
            reminderJob.JobDataMap.Put("groupId", group.Id);
            reminderJob.JobDataMap.Put("groupRepository", _groupRepository);
            reminderJob.JobDataMap.Put("apprenticeRepository", _apprenticeRepository);
            reminderJob.JobDataMap.Put("logRepository", _logRepository);
            reminderJob.JobDataMap.Put("weeksRepository", _weeksRepository);

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

        updateApprentices.JobDataMap.Put("apprenticeRepository", _apprenticeRepository);
        updateApprentices.JobDataMap.Put("groupRepository", _groupRepository);
        updateApprentices.JobDataMap.Put("logRepository", _logRepository);
        updateApprentices.JobDataMap.Put("weeksRepository", _weeksRepository);


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
        CommandCreator commandCreator = new CommandCreator(_client);
        await commandCreator.CreateCommands();
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "azubi":
                _apprenticeController.ApprenticeCommandHandler(command);
                break;
            case "gruppe":
                _groupController.GroupCommandHandler(command);
                break;
            case "wer":
                _berichtsheftController.BerichtsheftCommandHandler(command);
                break;
            case "woche":
                _weekController.WeekCommandHandler(command);
                break;
        }
    }

    private async Task ModalSubmittedHandler(SocketModal modal)
    {
        if (modal.Data.CustomId.Contains("ApprenticeMenu"))
        {
            _apprenticeController.ApprenticeModalHandler(modal);
        }

        if (modal.Data.CustomId.Contains("GroupMenu"))
        {
            _groupController.GroupModalHandler(modal);
        }
    }

    public async Task MyMessageComponentHandler(SocketMessageComponent component)
    {
        if (component.Data.CustomId.Contains("Apprentice"))
        {
            _apprenticeController.ApprenticeMessageComponentHandler(component);
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