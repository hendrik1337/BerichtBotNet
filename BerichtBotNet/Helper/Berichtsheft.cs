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
                  logs.Apprentice.Group.Name == groupName
            select logs).ToList();

        if (log.FirstOrDefault() != null)
        {
            return log.First().Apprentice;
        }

        throw new ApprenticeNotFoundException();
    }

    public Apprentice? GetCurrentBerichtsheftWriterOfGroup(int groupId)
    {
        List<Apprentice> apprenticesOfGroup = _apprenticeRepository.GetApprenticesInSameGroupByGroupId(groupId);
        apprenticesOfGroup = FilterApprenticesBySkipCount(apprenticesOfGroup, false);

        if (apprenticesOfGroup.FirstOrDefault() == null)
        {
            throw new GroupIsEmptyException();
        }

        var logs = _logRepository.GetLogsOfGroup(groupId);

        // Checks if every Apprentice has wrote before
        var apprenticesThatNeverWrote = GetApprenticesThatNeverWrote(apprenticesOfGroup, logs);
        if (apprenticesThatNeverWrote.FirstOrDefault() != null)
        {
            return apprenticesThatNeverWrote.First();
        }

        // Checks which Apprentice hasn't wrote the longest and is not being skipped
        var filteredList = FilterApprenticesFromLogBySkipped(logs, false);

        var oldestEntry = filteredList.MinBy(log => log.Timestamp); // Get the first (oldest) log entry

        return oldestEntry.Apprentice;
    }

    public List<Apprentice> GetApprenticesThatNeverWrote(List<Apprentice> apprenticesOfGroup, List<Log> logs)
    {
        var apprenticeIdsThatWrote = logs.Select(log => log.Apprentice.Id).Distinct().ToList();
        var apprenticesThatNeverWrote = apprenticesOfGroup.Where(apprentice => !apprenticeIdsThatWrote.Contains(apprentice.Id)).ToList();

        return apprenticesThatNeverWrote;
    }
    
    public List<Apprentice> FilterApprenticesBySkipCount(List<Apprentice> apprentices, bool skipped)
    {
        return apprentices.Where(a => a.Skipped == skipped).ToList();
    }

    public List<Log> FilterApprenticesFromLogBySkipped(List<Log> logs, bool skipped)
    {
        return logs
            .Where(log =>
                log.Apprentice.Skipped == skipped) // Filter logs based on SkipCount
            .GroupBy(log => log.Apprentice.Id) // Group logs by Apprentice
            .Select(group =>
                group.OrderByDescending(log => log.Timestamp).First()) // Select the most recent log for each group
            .ToList();
    }

    // Diese Methode dient dazu, die Reihenfolge der Auszubildenden im  Berichtsheft-Schreiben aus einer bestimmten Gruppe zu sortieren.
    public (List<Apprentice>?, List<Apprentice>?) BerichtsheftOrder(Group group)
    {
        // Die Liste der Auszubildenden, die zur angegebenen Gruppe gehören, wird abgerufen.
        List<Apprentice> apprenticesOfGroup = _apprenticeRepository.GetApprenticesInSameGroupByGroupId(group.Id);

        // Die Logs, die mit der Gruppe verknüpft sind, werden abgerufen.
        var logs = _logRepository.GetLogsOfGroup(group.Id);

        // Auszubildende in der Gruppe identifizieren, die noch nie einen Bericht geschrieben haben.
        List<Apprentice> apprenticesOfGroupThatNeverWrote = GetApprenticesThatNeverWrote(apprenticesOfGroup, logs);

        List<Apprentice> apprenticesOrderNotSkipped = new List<Apprentice>();
        List<Apprentice> apprenticesOrderSkipped = new List<Apprentice>();

        // Nicht übersprungene Auszubildende, die noch nie das Berichtsheft geschrieben , in die Reihenfolge aufnehmen.
        apprenticesOrderNotSkipped = apprenticesOrderNotSkipped
            .Concat(FilterApprenticesBySkipCount(apprenticesOfGroupThatNeverWrote, false)).ToList();

        // Auszubildende hinzufügen, die nicht übersprungen werden, aber schonmal ein Berichtsheft geschrieben haben, zur Reihenfolge.
        foreach (var log in FilterApprenticesFromLogBySkipped(logs, false))
        {
            apprenticesOrderNotSkipped.Add(log.Apprentice);
        }

        // Übersprungene Auszubildende, die noch nie das Berichtsheft geschrieben haben, in die Reihenfolge aufnehmen.
        apprenticesOrderSkipped = apprenticesOrderSkipped
            .Concat(FilterApprenticesBySkipCount(apprenticesOfGroupThatNeverWrote, true)).ToList();

        // Auszubildende hinzufügen, die übersprungen werden aber schonmal das Berichtsheft geschrieben haben, zur Reihenfolge.
        foreach (var log in FilterApprenticesFromLogBySkipped(logs, true))
        {
            apprenticesOrderSkipped.Add(log.Apprentice);
        }
        
        return (apprenticesOrderNotSkipped, apprenticesOrderSkipped);
    }

    public void CurrentBerichsheftWriterWrote(int groupId)
    {
        Apprentice currentApprentice = GetCurrentBerichtsheftWriterOfGroup(groupId);

        var log = new Log()
        {
            Apprentice = currentApprentice,
            Timestamp = DateTime.Now.ToUniversalTime(),
            BerichtheftNummer = WeekHelper.GetBerichtsheftNumber(currentApprentice.Group.StartOfApprenticeship, DateTime.Now)
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
            if (WeekHelper.DateTimeToCalendarWeekYearCombination(date.SkippedWeek).Equals(currentCalendarWeek))
            {
                return $"Diese Woche {berichtsheftNumberPlusCw} muss kein Berichtsheft geschrieben werden.";
            }
        }

        try
        {
            Apprentice? currentBerichtsheftWriter =
                GetCurrentBerichtsheftWriterOfGroup(group.Id);
            return
                $"Azubi: {currentBerichtsheftWriter.Name} muss diese Woche {berichtsheftNumberPlusCw} das Berichtsheft schreiben.";
        }
        catch (GroupIsEmptyException ignored)
        {
            return $"Es wurde keine Person gefunden, die das  Berichtsheft schreiben kann {berichtsheftNumberPlusCw}";
        }
    }

    public Apprentice GetBerichtsheftWriterOfDate(Group group, DateTime date)
    {
        var logs = _logRepository.GetLogsOfGroup(group.Id);

        foreach (var log in logs)
        {
            string logCw = WeekHelper.DateTimeToCalendarWeekYearCombination(log.Timestamp);
            string dateCw = WeekHelper.DateTimeToCalendarWeekYearCombination(date);
            if (logCw.Equals(dateCw))
            {
                return log.Apprentice;
            }
        }

        throw new BerichtsheftNotFound();
    }
    public Apprentice GetBerichtsheftWriterOfNumber(Group group, int number)
    {
        var logList = _logRepository.GetLogsOfGroup(group.Id);
        var log = from logs in logList
            where logs.BerichtheftNummer == number
            select logs;

        if (log.FirstOrDefault() is not null) return log.First().Apprentice;

        throw new BerichtsheftNotFound();
    }
}