using BerichtBotNet.Data;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;

namespace BerichtBotNet.Helper;

public class Berichtsheft
{
    private readonly ApprenticeRepository _apprenticeRepository;
    private readonly LogRepository _logRepository;
    private readonly SkippedWeeksRepository _weeksRepository;

    public Berichtsheft(ApprenticeRepository apprenticeRepository, LogRepository logRepository,
        SkippedWeeksRepository weeksRepository)
    {
        _apprenticeRepository = apprenticeRepository;
        _logRepository = logRepository;
        _weeksRepository = weeksRepository;
    }

    public Apprentice GetBerichtsheftWriterOfBerichtsheftNumber(int number, string groupName)
    {
        var log = (from logs in _logRepository.GetAllLogs()
            where logs.BerichtheftNummer == number &&
                  _apprenticeRepository.GetApprentice(logs.ApprenticeId).Group.Name == groupName
            select logs).ToList();

        if (log.FirstOrDefault() != null)
        {
            return _apprenticeRepository.GetApprentice(log.First().ApprenticeId);
        }

        throw new ApprenticeNotFoundException();
    }

    private List<Apprentice> GetNonSkippedApprentices(List<Apprentice> apprentices)
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

    public Apprentice? GetCurrentBerichtsheftWriterOfGroup(int groupId)
    {
        List<Apprentice> apprenticesOfGroup = _apprenticeRepository.GetApprenticesInSameGroupByGroupId(groupId);
        apprenticesOfGroup = GetNonSkippedApprentices(apprenticesOfGroup);

        if (apprenticesOfGroup.FirstOrDefault() == null)
        {
            throw new GroupIsEmptyException();
        }

        var logs = _logRepository.GetLogsOfGroup(groupId);

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
                if (_apprenticeRepository.GetApprentice(log.ApprenticeId).Id == apprentice.Id)
                {
                    hasWroteBefore = true;
                    break;
                }
            }

            if (!hasWroteBefore) return apprentice;
        }

        // Checks which Apprentice hasn't wrote the longest and is not being skipped
        var filteredList = logs
            .Where(log =>
                _apprenticeRepository.GetApprentice(log.ApprenticeId).SkipCount == 0) // Filter logs based on SkipCount
            .GroupBy(log => _apprenticeRepository.GetApprentice(log.ApprenticeId)) // Group logs by Apprentice
            .Select(group =>
                group.OrderByDescending(log => log.Timestamp).First()) // Select the most recent log for each group
            .ToList();

        var oldestEntry = filteredList.MinBy(log => log.Timestamp); // Get the first (oldest) log entry

        return _apprenticeRepository.GetApprentice(oldestEntry.ApprenticeId);
    }

    public void CurrentBerichsheftWriterWrote(int groupId)
    {
        Apprentice currentApprentice = GetCurrentBerichtsheftWriterOfGroup(groupId);

        var log = new Log()
        {
            ApprenticeId = currentApprentice.Id,
            Timestamp = DateTime.Now.ToUniversalTime(),
            BerichtheftNummer = 1337
        };

        _logRepository.CreateLog(log);
    }

    public string CurrentBerichtsheftWriterMessage(Group group)
    {
        List<SkippedWeeks> skippedWeeksList = _weeksRepository.GetByGroupId(int.Parse(group.Id.ToString()));
        string currentCalendarWeek = WeekHelper.DateTimeToCalendarWeekYearCombination(DateTime.Now);
        int berichtsheftNumber = WeekHelper.GetBerichtsheftNumber(group.StartOfApprenticeship, DateTime.Now);
        string berichtsheftNumberPlusCw = $"(Nr: {berichtsheftNumber}, {currentCalendarWeek})";

        foreach (var date in skippedWeeksList)
        {
            if (WeekHelper.DateTimeToCalendarWeekYearCombination(date.SkippedWeek) == currentCalendarWeek)
            {
                return $"Diese Woche {berichtsheftNumberPlusCw} muss kein Berichtsheft geschrieben werden.";
            }
        }

        try
        {
            Apprentice? currentBerichtsheftWriter =
                GetCurrentBerichtsheftWriterOfGroup(group.Id);
            return $"Azubi: {currentBerichtsheftWriter.Name} ist diese Woche {berichtsheftNumberPlusCw} dran.";
        }
        catch (GroupIsEmptyException ignored)
        {
            return $"Es wurde keine Person gefunden, die das  Berichtsheft schreiben kann {berichtsheftNumberPlusCw}";
        }
    }
}