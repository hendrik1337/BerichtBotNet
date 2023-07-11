using BerichtBotNet.Data;
using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public class BerichtsheftCommands
{
    public static void BerichtsheftCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "wer":
                SendCurrentBerichtsheftWriter(command);
                break;
        }
    }

    private static async void SendCurrentBerichtsheftWriter(SocketSlashCommand command)
    {
        using BerichtBotContext context = new BerichtBotContext();
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        Apprentice? requester = apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        SkippedWeeksRepository weeksRepository = new SkippedWeeksRepository(context);

        if (requester == null)
        {
            await command.RespondAsync(
                $"Du wurdest nicht in der Datenbank gefunden. Registriere dich mit '/azubi hinzufügen'");
            return;
        }

        List<SkippedWeeks> skippedWeeksList = weeksRepository.GetByGroupId(int.Parse(requester.Group.Id.ToString()));
        string currentCalendarWeek = WeekHelper.DateTimeToCalendarWeekYearCombination(DateTime.Now);
        int berichtsheftNumber = WeekHelper.GetBerichtsheftNumber(requester.Group.StartOfApprenticeship, DateTime.Now);
        string berichtsheftNumberPlusCw = $"(Nr: {berichtsheftNumber}, {currentCalendarWeek})";
        Console.WriteLine(requester.Group.StartOfApprenticeship);

        foreach (var date in skippedWeeksList)
        {
            if (WeekHelper.DateTimeToCalendarWeekYearCombination(date.SkippedWeek) == currentCalendarWeek)
            {
                await command.RespondAsync(
                    $"Diese Woche {berichtsheftNumberPlusCw} muss kein Berichtsheft geschrieben werden.");
            }
        }

        Apprentice? currentBerichtsheftWriter = Berichtsheft.GetCurrentBerichtsheftWriterOfGroup(requester.Group.Id);

        if (currentBerichtsheftWriter == null)
        {
            await command.RespondAsync(
                $"Es wurde kein Azubi gefunden, der das Berichtsheft schreiben kann {berichtsheftNumberPlusCw}");
            return;
        }

        await command.RespondAsync(
            $"Azubi: {currentBerichtsheftWriter.Name} ist diese Woche {berichtsheftNumberPlusCw} dran.");
    }
}