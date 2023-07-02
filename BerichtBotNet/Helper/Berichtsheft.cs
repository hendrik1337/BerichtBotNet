using BerichtBotNet.Data;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;

namespace BerichtBotNet.Helper;

public class Berichtsheft
{
    public static Apprentice GetBerichtsheftWriterOfBerichtsheftNumber(int number, string groupName)
    {
        using BerichtBotContext context = new BerichtBotContext();
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);

        var log = (from logs in context.Logs
            where logs.BerichtheftNummer == number &&
                  apprenticeRepository.GetApprentice(logs.ApprenticeId).Group.Name == groupName
            select logs).ToList();

        if (log.FirstOrDefault() != null)
        {
            return apprenticeRepository.GetApprentice(log.First().ApprenticeId);
        }

        throw new ApprenticeNotFoundException();
    }

    private static List<Apprentice> GetNonSkippedApprentices(List<Apprentice> apprentices)
    {
        List<Apprentice> filteredApprentices = new List<Apprentice>();
        foreach (var apprentice in apprentices)
        {
            if (apprentice.SkipCount == 0)
            {
                Console.WriteLine($"Adding apprentice: {apprentice.Name}");
                filteredApprentices.Add(apprentice);
            }
        }

        return filteredApprentices;
    }
    
    public static Apprentice GetCurrentBerichtsheftWriterOfGroup(int groupId)
    {
        using BerichtBotContext context = new BerichtBotContext();
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        LogRepository logRepository = new LogRepository(context);

        List<Apprentice> apprenticesOfGroup = apprenticeRepository.GetApprenticesInSameGroupByGroupId(groupId);
        apprenticesOfGroup = GetNonSkippedApprentices(apprenticesOfGroup);

        if (apprenticesOfGroup.FirstOrDefault() == null)
        {
            throw new GroupIsEmptyException();
        }

        var logs = logRepository.GetLogsOfGroup(groupId);

        // Returns First Apprentice if none have wrote before
        if (logs.FirstOrDefault() == null)
        {
            return apprenticesOfGroup.First();
        }

        // Checks if every Apprentice has wrote before
        foreach (var apprentice in apprenticesOfGroup)
        {
            bool hasWroteBefore = false;
            foreach (var log in logs)
            {
                if (apprenticeRepository.GetApprentice(log.ApprenticeId).Id == apprentice.Id)
                {
                    hasWroteBefore = true;
                    break;
                }
            }

            if (!hasWroteBefore) return apprentice;
        }

        // Checks which Apprentice hasn't wrote the longest and is not being skipped
        var filteredList = logs
            .Where(log => apprenticeRepository.GetApprentice(log.ApprenticeId).SkipCount == 0) // Filter logs based on SkipCount
            .GroupBy(log => apprenticeRepository.GetApprentice(log.ApprenticeId)) // Group logs by Apprentice
            .Select(group => group.OrderByDescending(log => log.Timestamp).First()) // Select the most recent log for each group
            .ToList();

        var oldestEntry = filteredList.MinBy(log => log.Timestamp); // Get the first (oldest) log entry

        return apprenticeRepository.GetApprentice(oldestEntry.ApprenticeId);


    }

    public static void CurrentBerichsheftWriterWrote(int groupId)
    {
        using BerichtBotContext context = new BerichtBotContext();
        LogRepository logRepository = new LogRepository(context);
        Apprentice currentApprentice = GetCurrentBerichtsheftWriterOfGroup(groupId);

        var log = new Log()
        {
            ApprenticeId = currentApprentice.Id,
            Timestamp = DateTime.Now.ToUniversalTime(),
            BerichtheftNummer = 1337
        };

        logRepository.CreateLog(log);
    }
}