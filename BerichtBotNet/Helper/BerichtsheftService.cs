using BerichtBotNet.Data;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord;

namespace BerichtBotNet.Helper;

public class BerichtsheftService
{
    private readonly ApprenticeRepository _apprenticeRepository;
    private readonly LogRepository _logRepository;
    private readonly SkippedWeeksRepository _weeksRepository;

    public BerichtsheftService(ApprenticeRepository apprenticeRepository, LogRepository logRepository,
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
        // Get the list of apprentices belonging to the specified group
        List<Apprentice> apprenticesOfGroup = _apprenticeRepository.GetApprenticesInSameGroupByGroupId(group.Id);

        // Get the logs associated with the group
        var logs = _logRepository.GetLogsOfGroup(group.Id);

        // Identify apprentices in the group who have never written a Berichtsheft
        List<Apprentice> apprenticesThatNeverWrote = GetApprenticesThatNeverWrote(apprenticesOfGroup, logs);

        // Initialize lists for apprentices who are not skipped and apprentices who are skipped
        List<Apprentice> apprenticesOrderNotSkipped = new List<Apprentice>();
        List<Apprentice> apprenticesOrderSkipped = new List<Apprentice>();

        // Add apprentices who are not skipped and have never written a Berichtsheft to the not skipped order list
        apprenticesOrderNotSkipped.AddRange(FilterApprenticesBySkipCount(apprenticesThatNeverWrote, false));
        var notSkippedFromLog = FilterApprenticesFromLogBySkipped(logs, false);
        apprenticesOrderNotSkipped.AddRange(notSkippedFromLog.Select(log => log.Apprentice));

        // Add apprentices who are skipped and have never written a Berichtsheft to the skipped order list
        apprenticesOrderSkipped.AddRange(FilterApprenticesBySkipCount(apprenticesThatNeverWrote, true));
        var skippedFromLog = FilterApprenticesFromLogBySkipped(logs, true);
        apprenticesOrderSkipped.AddRange(skippedFromLog.Select(log => log.Apprentice));

        // Return the lists of apprentices in the order, with null values if the lists are empty
        return (apprenticesOrderNotSkipped.Count > 0 ? apprenticesOrderNotSkipped : null,
            apprenticesOrderSkipped.Count > 0 ? apprenticesOrderSkipped : null);
    }


    public void CurrentBerichsheftWriterWrote(int groupId)
    {
        Apprentice currentApprentice = GetCurrentBerichtsheftWriterOfGroup(groupId);

        var log = new Log()
        {
            Apprentice = currentApprentice,
            Timestamp = DateTime.Now.ToUniversalTime(),
            BerichtheftNummer = WeekHelper.GetBerichtsheftNumber(currentApprentice.Group.StartOfApprenticeship, DateTime.Now),
            Group = currentApprentice.Group
        };

        _logRepository.CreateLog(log);
    }

    public string CurrentBerichtsheftWriterMessage(Group group, bool mention)
    {
        List<SkippedWeeks> skippedWeeksList = _weeksRepository.GetByGroupId(int.Parse(group.Id.ToString()));
        string currentCalendarWeek = WeekHelper.DateTimeToCalendarWeekYearCombination(DateTime.Now);
        int berichtsheftNumber = WeekHelper.GetBerichtsheftNumber(group.StartOfApprenticeship, DateTime.Now);
        string berichtsheftNumberPlusCw = $"(Nr: {berichtsheftNumber}, {currentCalendarWeek})";

        bool isSkippedWeek = skippedWeeksList.Any(date =>
            WeekHelper.DateTimeToCalendarWeekYearCombination(date.SkippedWeek).Equals(currentCalendarWeek));

        if (isSkippedWeek)
        {
            return $"Diese Woche {berichtsheftNumberPlusCw} muss kein Berichtsheft geschrieben werden.";
        }

        try
        {
            Apprentice? currentBerichtsheftWriter = GetCurrentBerichtsheftWriterOfGroup(group.Id);
            string messagePrefix = mention ? $"Azubi: <@!{currentBerichtsheftWriter.DiscordUserId}>" : $"Azubi: {currentBerichtsheftWriter.Name}";
            return $"{messagePrefix} muss diese Woche {berichtsheftNumberPlusCw} das Berichtsheft schreiben.";
        }
        catch (GroupIsEmptyException)
        {
            return $"Es wurde keine Person gefunden, die das Berichtsheft schreiben kann {berichtsheftNumberPlusCw}";
        }
    }


    public Apprentice GetBerichtsheftWriterOfDate(Group group, DateTime date)
    {
        var logs = _logRepository.GetLogsOfGroup(group.Id);
        string dateCw = WeekHelper.DateTimeToCalendarWeekYearCombination(date);

        var writerLog = logs.FirstOrDefault(log =>
            WeekHelper.DateTimeToCalendarWeekYearCombination(log.Timestamp).Equals(dateCw));

        if (writerLog != null)
        {
            return writerLog.Apprentice;
        }

        throw new BerichtsheftNotFound();
    }

    public Apprentice GetBerichtsheftWriterOfNumber(Group group, int number)
    {
        var logList = _logRepository.GetLogsOfGroup(group.Id);

        var writerLog = logList.FirstOrDefault(log => log.BerichtheftNummer == number);

        if (writerLog != null)
        {
            return writerLog.Apprentice;
        }

        throw new BerichtsheftNotFound();
    }

}