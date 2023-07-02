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
        using BerichtBotContext dataContext = new BerichtBotContext();
        GroupRepository groupRepository = new GroupRepository(dataContext);

        var allGroups = groupRepository.GetAllGroups();
        foreach (var group in allGroups)
        {
            Berichtsheft.CurrentBerichsheftWriterWrote(group.Id);
        }
        Console.WriteLine("Current Berichtsheft writer has been updated in every group!");
        return Task.CompletedTask;
    }
}