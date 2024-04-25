using BerichtBotNet.Data;
using BerichtBotNet.Discord.Controller;
using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Reminders;
using BerichtBotNet.Repositories;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace BerichtBotNet;

class BerichtBotNet
{
    private DiscordSocketClient _client = null!;
    private IScheduler _scheduler = null!;
    private CancellationTokenSource _cancellationTokenSource;
    private IHost _builder;

    private ApprenticeRepository _apprenticeRepository;
    private GroupRepository _groupRepository;
    private LogRepository _logRepository;
    private SkippedWeeksRepository _weeksRepository;

    private ReminderHelper _reminderHelper;
    private BerichtsheftService _berichtsheftService;

    private BerichtsheftController _berichtsheftController;
    private ApprenticeController _apprenticeController;
    private GroupController _groupController;
    private WeekController _weekController;
    private HelpController _helpController;

    public static Task Main(string[] args) => new BerichtBotNet().MainAsync();

    public async Task MainAsync()
    {
        var serviceProvider = new ServiceCollection()
            .AddDbContext<BerichtBotContext>(options =>
                options.UseNpgsql(
                    Environment.GetEnvironmentVariable(
                        "PostgreSQLBerichtBotConnection")))
            .BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BerichtBotContext>();

            // Apply pending migrations
            dbContext.Database.Migrate();
        }

        _cancellationTokenSource = new CancellationTokenSource();

        _client = new DiscordSocketClient();

        // _client.Log += Log;

        _client.SlashCommandExecuted += SlashCommandHandler;

        _client.Ready += Client_Ready;

        _client.ModalSubmitted += ModalSubmittedHandler;

        _client.ButtonExecuted += MyMessageComponentHandler;

        _client.SelectMenuExecuted += MyMessageComponentHandler;

        var token = Environment.GetEnvironmentVariable("DiscordToken");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        InitializeDependencies();

        await LoadTaskScheduler();
    }


    private async void InitializeDependencies()
    {
        BerichtBotContext context = new BerichtBotContext();
        // Load the Task Scheduler
        _builder = Host.CreateDefaultBuilder()
            .ConfigureServices((cxt, services) =>
            {
                services.AddQuartz(q => { q.UseMicrosoftDependencyInjectionJobFactory(); });
                services.AddQuartzHostedService(opt => { opt.WaitForJobsToComplete = true; });
            }).Build();
        var schedulerFactory = _builder.Services.GetRequiredService<ISchedulerFactory>();
        _scheduler = await schedulerFactory.GetScheduler();

        _apprenticeRepository = new ApprenticeRepository(context);
        _groupRepository = new GroupRepository(context);
        _logRepository = new LogRepository(context);
        _weeksRepository = new SkippedWeeksRepository(context);
        _berichtsheftService = new BerichtsheftService(_apprenticeRepository, _logRepository, _weeksRepository);
        _reminderHelper = new ReminderHelper(_client, _apprenticeRepository, _groupRepository, _logRepository,
            _weeksRepository, _scheduler, _berichtsheftService);
        _apprenticeController = new ApprenticeController(_apprenticeRepository, _groupRepository, _berichtsheftService);
        _berichtsheftController = new BerichtsheftController(_apprenticeRepository, _weeksRepository, _logRepository);
        _groupController = new GroupController(_groupRepository, _apprenticeRepository, _reminderHelper);
        _weekController = new WeekController(_apprenticeRepository, _weeksRepository);
        _helpController = new HelpController();
    }

    private async Task LoadTaskScheduler()
    {
        List<Group> groups = _groupRepository.GetAllGroups();

        // Load the Task Scheduler


        // Create Custom Reminder Job for every group
        foreach (var group in groups)
        {
            _reminderHelper.CreateReminderForGroup(group);
            _reminderHelper.CreateBerichtsheftCreatorTask(group);
        }


        var updateApprentices = JobBuilder.Create<UpdateCurrentApprenticeTask>()
            .WithIdentity("updateApprentices", "group2")
            .Build();

        updateApprentices.JobDataMap.Put("apprenticeRepository", _apprenticeRepository);
        updateApprentices.JobDataMap.Put("groupRepository", _groupRepository);
        updateApprentices.JobDataMap.Put("logRepository", _logRepository);
        updateApprentices.JobDataMap.Put("weeksRepository", _weeksRepository);
        updateApprentices.JobDataMap.Put("berichtsheftService", _berichtsheftService);


        var updateApprenticesTrigger = TriggerBuilder.Create()
            .WithIdentity("myTrigger2", "group2")
            .StartNow()
            .WithSchedule(CronScheduleBuilder
                .WeeklyOnDayAndHourAndMinute(DayOfWeek.Sunday, 20, 0))
            .Build();


        await _scheduler.ScheduleJob(updateApprentices, updateApprenticesTrigger);
        
        await _scheduler.Start();

        await _builder.RunAsync();
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
            case "berichtsheft":
                _berichtsheftController.BerichtsheftCommandHandler(command);
                break;
            case "woche":
                _weekController.WeekCommandHandler(command);
                break;
            case "hilfe":
                _helpController.HelpCommandHandler(command);
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
        else if (component.Data.CustomId.Contains("Group"))
        {
            _groupController.GroupMessageComponentHandler(component);
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