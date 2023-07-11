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

        ApprenticeRepository _apprenticeRepository = (ApprenticeRepository)(dataMap)["_apprenticeRepository"];
        GroupRepository groupRepository = (GroupRepository)(dataMap)["_groupRepository"];
        if (groupRepository == null) throw new ArgumentNullException(nameof(groupRepository));
        LogRepository logRepository = (LogRepository)(dataMap)["_logRepository"];

        Berichtsheft berichtsheft = new Berichtsheft(_apprenticeRepository, logRepository);

        var allGroups = groupRepository.GetAllGroups();
        foreach (var group in allGroups)
        {
            berichtsheft.CurrentBerichsheftWriterWrote(group.Id);
        }
        Console.WriteLine("Current Berichtsheft writer has been updated in every group!");
        return Task.CompletedTask;
    }
}