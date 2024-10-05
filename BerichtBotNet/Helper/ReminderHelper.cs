using BerichtBotNet.Data;
using BerichtBotNet.Reminders;
using BerichtBotNet.Repositories;
using Discord.WebSocket;
using Quartz;
using Quartz.Impl.Matchers;

namespace BerichtBotNet.Helper;

public class ReminderHelper
{
    private readonly DiscordSocketClient _client;
    private readonly ApprenticeRepository _apprenticeRepository;
    private readonly GroupRepository _groupRepository;
    private readonly LogRepository _logRepository;
    private readonly SkippedWeeksRepository _weeksRepository;
    private readonly IScheduler _scheduler;
    private readonly BerichtsheftService _berichtsheftService;

    public ReminderHelper(DiscordSocketClient client, ApprenticeRepository apprenticeRepository,
        GroupRepository groupRepository, LogRepository logRepository, SkippedWeeksRepository weeksRepository,
        IScheduler scheduler, BerichtsheftService berichtsheftService)
    {
        _client = client;
        _apprenticeRepository = apprenticeRepository;
        _groupRepository = groupRepository;
        _logRepository = logRepository;
        _weeksRepository = weeksRepository;
        _scheduler = scheduler;
        _berichtsheftService = berichtsheftService;
    }

    public async void CreateReminderForGroup(Group group)
    {
        Console.WriteLine("Creating Reminder");
        Console.WriteLine(group.Name);
        // Define the job and tie it to our ReminderTasks class
        var reminderJob = JobBuilder.Create<ReminderTasks>()
            .WithIdentity($"myGroupReminderJob{group.Name}", $"groupReminder{group.Name}")
            .Build();

        reminderJob.JobDataMap.Put("discord", _client);
        reminderJob.JobDataMap.Put("groupId", group.Id);
        reminderJob.JobDataMap.Put("groupRepository", _groupRepository);
        reminderJob.JobDataMap.Put("apprenticeRepository", _apprenticeRepository);
        reminderJob.JobDataMap.Put("logRepository", _logRepository);
        reminderJob.JobDataMap.Put("weeksRepository", _weeksRepository);
        reminderJob.JobDataMap.Put("berichtsheftService", _berichtsheftService);

        var reminderTrigger = TriggerBuilder.Create()
            .WithIdentity($"myGroupReminderTrigger{group.Name}", $"groupReminder{group.Name}")
            .StartNow()
            .WithSchedule(CronScheduleBuilder
                .WeeklyOnDayAndHourAndMinute(
                    group.ReminderDayOfWeek,
                    group.ReminderTime.ToLocalTime().Hour,
                    group.ReminderTime.ToLocalTime().Minute))
            .Build();

        // Retrieve the trigger's identity
        var triggerKey = new TriggerKey($"myGroupReminderTrigger{group.Name}",
            $"groupReminder{group.Name}");

        // Check if the trigger exists
        if (await _scheduler.CheckExists(triggerKey))
        {
            // Unschedule the job
            Console.WriteLine("Job exists, Rescheduling");
            await _scheduler.RescheduleJob(triggerKey, reminderTrigger);
        }
        else
        {
            Console.WriteLine("creating new Job");
            await _scheduler.ScheduleJob(reminderJob, reminderTrigger);
        }
        
        // Get job groups
        var groups = await _scheduler.GetJobGroupNames();

// Loop through each group
        foreach (var jobgroup in groups)
        {
            // Get job keys within a group
            var jobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobgroup));

            // Loop through each job
            foreach (var jobKey in jobKeys)
            {
                var detail = await _scheduler.GetJobDetail(jobKey);

                // Here's your job detail
                Console.WriteLine(
                    $"Job found! Key: {detail.Key}, Description: {detail.Description}, Job Type: {detail.JobType}");
            }
        }
    }
}