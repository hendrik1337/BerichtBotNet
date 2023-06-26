using Quartz;

namespace BerichtBotNet.Reminders;

public class ReminderTasks : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        // Your code here
        // This function will be executed every Monday at 8 am
        Console.WriteLine("Moin Moin, meine aktiven Freunde!");
        return Task.CompletedTask;
    }

}