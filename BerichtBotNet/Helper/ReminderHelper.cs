using BerichtBotNet.Data;
using BerichtBotNet.Reminders;
using BerichtBotNet.Repositories;
using Discord.WebSocket;
using Quartz;

namespace BerichtBotNet.Helper;

public class ReminderHelper
{
    public static async void CreateReminderForGroup(Group group, DiscordSocketClient client,
        ApprenticeRepository apprenticeRepository,
        GroupRepository groupRepository, LogRepository logRepository, SkippedWeeksRepository weeksRepository,
        IScheduler scheduler)
    {
        // Define the job and tie it to our ReminderTasks class
        var reminderJob = JobBuilder.Create<ReminderTasks>()
            .WithIdentity($"myGroupReminderJob{group.Id.ToString()}", $"groupReminder{group.Id.ToString()}")
            .Build();

        reminderJob.JobDataMap.Put("discord", client);
        reminderJob.JobDataMap.Put("groupId", group.Id);
        reminderJob.JobDataMap.Put("groupRepository", groupRepository);
        reminderJob.JobDataMap.Put("apprenticeRepository", apprenticeRepository);
        reminderJob.JobDataMap.Put("logRepository", logRepository);
        reminderJob.JobDataMap.Put("weeksRepository", weeksRepository);

        var reminderTrigger = TriggerBuilder.Create()
            .WithIdentity($"myGroupReminderTrigger{group.Id.ToString()}", $"groupReminder{group.Id.ToString()}")
            .StartNow()
            .WithSchedule(CronScheduleBuilder
                .WeeklyOnDayAndHourAndMinute(
                    DayOfWeek.Wednesday,
                    group.ReminderTime.ToLocalTime().Hour,
                    group.ReminderTime.ToLocalTime().Minute))
            .Build();

        await scheduler.ScheduleJob(reminderJob, reminderTrigger);
    }
}