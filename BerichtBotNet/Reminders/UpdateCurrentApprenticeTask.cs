using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Quartz;

namespace BerichtBotNet.Reminders;

public class UpdateCurrentApprenticeTask: IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        // Your code here
        // This function will be executed every Sunday at 20:00
        var dataMap = context.MergedJobDataMap;

        ApprenticeRepository apprenticeRepository = (ApprenticeRepository)(dataMap)["apprenticeRepository"];
        GroupRepository groupRepository = (GroupRepository)(dataMap)["groupRepository"];
        LogRepository logRepository = (LogRepository)(dataMap)["logRepository"];
        SkippedWeeksRepository weeksRepository = (SkippedWeeksRepository)(dataMap)["weeksRepository"];

        BerichtsheftService berichtsheftService = new BerichtsheftService(apprenticeRepository, logRepository, weeksRepository);

        var allGroups = groupRepository.GetAllGroups();
        foreach (var group in allGroups)
        {
            berichtsheftService.CurrentBerichsheftWriterWrote(group.Id);
        }
        Console.WriteLine("Current Berichtsheft writer has been updated in every group!");
        return Task.CompletedTask;
    }
}