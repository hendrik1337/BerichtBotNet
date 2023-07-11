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

        // Checks if every Apprentice has wrote before
        foreach (var apprentice in GetNonSkippedApprenticesThatNeverWrote(apprenticesOfGroup))
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

            if (!hasWroteBefore && apprentice.SkipCount == 0) return apprentice;
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

    public List<Apprentice> GetApprenticesThatNeverWrote(List<Apprentice> apprenticesOfGroup, List<Log> logs)
    {
        List<Apprentice> apprenticesThatNeverWrote = new List<Apprentice>();

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

            if (!hasWroteBefore) apprenticesThatNeverWrote.Add(apprentice);
        }

        return apprenticesThatNeverWrote;
    }

    public List<Apprentice> GetNonSkippedApprenticesThatNeverWrote(List<Apprentice> apprenticesOfGroupThatNeverWrote)
    {
        List<Apprentice> apprenticesThatNeverWrote = new List<Apprentice>();
        // Checks if every Apprentice has wrote before
        foreach (var apprentice in apprenticesOfGroupThatNeverWrote)
        {
            if (apprentice.SkipCount == 0) apprenticesThatNeverWrote.Add(apprentice);
        }

        return apprenticesThatNeverWrote;
    }

    public List<Apprentice> GetSkippedApprenticesThatNeverWrote(List<Apprentice> apprenticesOfGroupThatNeverWrote)
    {
        List<Apprentice> apprenticesThatNeverWrote = new List<Apprentice>();

        // Checks if every Apprentice has wrote before
        foreach (var apprentice in apprenticesOfGroupThatNeverWrote)
        {
            if (apprentice.SkipCount == 1)
            {
                apprenticesThatNeverWrote.Add(apprentice);
            }
        }

        return apprenticesThatNeverWrote;
    }

    public List<Log> GetNonSkippedApprenticesLog(List<Log> logs)
    {
        return logs
            .Where(log =>
                _apprenticeRepository.GetApprentice(log.ApprenticeId).SkipCount == 0) // Filter logs based on SkipCount
            .GroupBy(log => _apprenticeRepository.GetApprentice(log.ApprenticeId)) // Group logs by Apprentice
            .Select(group =>
                group.OrderByDescending(log => log.Timestamp).First()) // Select the most recent log for each group
            .ToList();
    }

    public List<Log> GetSkippedApprenticesLog(List<Log> logs)
    {
        return logs
            .Where(log =>
                _apprenticeRepository.GetApprentice(log.ApprenticeId).SkipCount != 0) // Filter logs based on SkipCount
            .GroupBy(log => _apprenticeRepository.GetApprentice(log.ApprenticeId)) // Group logs by Apprentice
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
            .Concat(GetNonSkippedApprenticesThatNeverWrote(apprenticesOfGroupThatNeverWrote)).ToList();

        // Auszubildende hinzufügen, die nicht übersprungen werden, aber schonmal ein Berichtsheft geschrieben haben, zur Reihenfolge.
        foreach (var log in GetNonSkippedApprenticesLog(logs))
        {
            apprenticesOrderNotSkipped.Add(_apprenticeRepository.GetApprentice(log.ApprenticeId));
        }

        // Übersprungene Auszubildende, die noch nie das Berichtsheft geschrieben haben, in die Reihenfolge aufnehmen.
        apprenticesOrderSkipped = apprenticesOrderNotSkipped
            .Concat(GetSkippedApprenticesThatNeverWrote(apprenticesOfGroupThatNeverWrote)).ToList();

        // Auszubildende hinzufügen, die übersprungen werden aber schonmal das Berichtsheft geschrieben haben, zur Reihenfolge.
        foreach (var log in GetSkippedApprenticesLog(logs))
        {
            apprenticesOrderSkipped.Add(_apprenticeRepository.GetApprentice(log.ApprenticeId));
        }
        
        return (apprenticesOrderNotSkipped, apprenticesOrderSkipped);
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
            return
                $"Azubi: {currentBerichtsheftWriter.Name} muss diese Woche {berichtsheftNumberPlusCw} das Berichtsheft schreiben.";
        }
        catch (GroupIsEmptyException ignored)
        {
            return $"Es wurde keine Person gefunden, die das  Berichtsheft schreiben kann {berichtsheftNumberPlusCw}";
        }
    }
}