using BerichtBotNet.Data;
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

            List<Apprentice> apprenticesOfGroup = apprenticeRepository.GetApprenticesInSameGroupByGroupId(group.Id);
            // TODO so machen, dass man auch dauerhaft übersprungen werden kann
            foreach (var apprentice in apprenticesOfGroup)
            {
                if (apprentice.Skipped)
                {
                    apprentice.Skipped = false;
                    apprenticeRepository.UpdateApprentice(apprentice);
                }
            }
        }
        Console.WriteLine("Current Berichtsheft writer has been updated in every group!");
        return Task.CompletedTask;
    }
}