using BerichtBotNet.Data;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public class BerichtsheftController
{
    private readonly ApprenticeRepository _apprenticeRepository;
    private readonly SkippedWeeksRepository _weeksRepository;
    private readonly LogRepository _logRepository;

    public BerichtsheftController(ApprenticeRepository apprenticeRepository, SkippedWeeksRepository weeksRepository,
        LogRepository logRepository)
    {
        _apprenticeRepository = apprenticeRepository;
        _weeksRepository = weeksRepository;
        _logRepository = logRepository;
    }

    public void BerichtsheftCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "wer":
                SendCurrentBerichtsheftWriter(command);
                break;
        }
    }

    private async void SendCurrentBerichtsheftWriter(SocketSlashCommand command)
    {
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());

        if (requester == null)
        {
            await command.RespondAsync(
                $"Du wurdest nicht in der Datenbank gefunden. Registriere dich mit '/azubi hinzufügen'");
            return;
        }

        List<SkippedWeeks> skippedWeeksList = _weeksRepository.GetByGroupId(int.Parse(requester.Group.Id.ToString()));
        string currentCalendarWeek = WeekHelper.DateTimeToCalendarWeekYearCombination(DateTime.Now);
        int berichtsheftNumber = WeekHelper.GetBerichtsheftNumber(requester.Group.StartOfApprenticeship, DateTime.Now);
        string berichtsheftNumberPlusCw = $"(Nr: {berichtsheftNumber}, {currentCalendarWeek})";

        foreach (var date in skippedWeeksList)
        {
            if (WeekHelper.DateTimeToCalendarWeekYearCombination(date.SkippedWeek) == currentCalendarWeek)
            {
                await command.RespondAsync(
                    $"Diese Woche {berichtsheftNumberPlusCw} muss kein Berichtsheft geschrieben werden.");
            }
        }

        Berichtsheft berichtsheft = new Berichtsheft(_apprenticeRepository, _logRepository);

        try
        {
            Apprentice? currentBerichtsheftWriter =
                berichtsheft.GetCurrentBerichtsheftWriterOfGroup(requester.Group.Id);
            await command.RespondAsync(
                $"Azubi: {currentBerichtsheftWriter.Name} ist diese Woche {berichtsheftNumberPlusCw} dran.");
        }
        catch (GroupIsEmptyException ignored)
        {
            await command.RespondAsync(
                $"Es wurde kein Azubi gefunden, der das Berichtsheft schreiben kann {berichtsheftNumberPlusCw}");
        }
    }
}