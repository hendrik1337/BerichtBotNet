using System.Globalization;
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
        switch (command.Data.Options.First().Name)
        {
            case "wer":
                SendCurrentBerichtsheftWriter(command);
                break;
            case "reihenfolge":
                SendBerichtsheftWriterReihenfolge(command);
                break;
            case "log":
                SendBerichtsheftLog(command);
                break;
        }
    }

    private async void SendBerichtsheftLog(SocketSlashCommand command)
    {
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        if (requester is null)
        {
            await command.RespondAsync(Constants.UserNotRegistered);
        }

        int limit = 10;
        try
        {
            limit = int.Parse(command.Data.Options.First().Options.First().Value.ToString());
        }
        catch (InvalidOperationException ignored)
        {
            
        }

        var logs = _logRepository.GetLogsOfGroup(requester.Group.Id, limit);

        string ans = $"Die letzten {limit} Log Einträge:\n\n";

        foreach (var log in logs)
        {
            ans += $"Berichtsheft: {log.BerichtheftNummer} " +
                   $"({WeekHelper.DateTimeToCalendarWeekYearCombination(log.Timestamp)}) " +
                   $"Azubi: {_apprenticeRepository.GetApprentice(log.ApprenticeId).Name}" +
                   $"\n";
        }

        await command.RespondAsync(ans);
    }

    private async void SendCurrentBerichtsheftWriter(SocketSlashCommand command)
    {
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());

        if (requester == null)
        {
            await command.RespondAsync(Constants.UserNotRegistered);
            return;
        }

        Berichtsheft berichtsheft = new Berichtsheft(_apprenticeRepository, _logRepository, _weeksRepository);

        if (command.Data.Options.First().Options.First().Value is null)
        {
            await command.RespondAsync(berichtsheft.CurrentBerichtsheftWriterMessage(requester.Group));
            return;
        }

        var value = command.Data.Options.First().Options.First().Value;
        switch (command.Data.Options.First().Options.First().Name)
        {
            case "nummer":
                await SendBerichtsheftWriterByNumber(command, value, berichtsheft, requester);
                break;
            case "datum":
                await SendBerichtsheftWriterByDate(command, value, berichtsheft, requester);
                break;
        }
    }

    private static async Task SendBerichtsheftWriterByDate(SocketSlashCommand command, object value,
        Berichtsheft berichtsheft, Apprentice requester)
    {
        IFormatProvider provider = Constants.CultureInfo;
        var isDate = DateTime.TryParse(value.ToString(), provider, DateTimeStyles.AssumeLocal,
            out DateTime date);
        if (!isDate)
        {
            await command.RespondAsync(
                $"Die Eingabe {value} konnte nicht in ein Datum umgewandelt werden.\nBitte gib ein Datum im Format DD/MM/YY(YY) an.");
            return;
        }

        string dateCw = WeekHelper.DateTimeToCalendarWeekYearCombination(date);

        try
        {
            Apprentice apprentice = berichtsheft.GetBerichtsheftWriterOfDate(requester.Group, date);
            string ans =
                $"Azubi: {apprentice.Name} musste das Berichtsheft {date:d} ({dateCw}) schreiben.";
            await command.RespondAsync(ans);
        }
        catch (BerichtsheftNotFound ignored)
        {
            await command.RespondAsync(
                $"Das Berichtsheft aus der Woche mit dem Datum: {date:d} ({dateCw}) wurde in der Datenbank nicht gefunden.");
        }
    }

    private static async Task SendBerichtsheftWriterByNumber(SocketSlashCommand command, object value,
        Berichtsheft berichtsheft, Apprentice requester)
    {
        var number = int.Parse(value.ToString()!);
        try
        {
            Apprentice apprentice = berichtsheft.GetBerichtsheftWriterOfNumber(requester.Group, number);
            string ans =
                $"Azubi: {apprentice.Name} musste das Berichtsheft {number} schreiben.";
            await command.RespondAsync(ans);
        }
        catch (BerichtsheftNotFound ignored)
        {
            await command.RespondAsync($"Das Berichtsheft {number} wurde in der Datenbank nicht gefunden.");
        }
    }

    private async void SendBerichtsheftWriterReihenfolge(SocketSlashCommand command)
    {
        Apprentice requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString())!;

        Berichtsheft berichtsheft = new Berichtsheft(_apprenticeRepository, _logRepository, _weeksRepository);
        var berichtsheftWriterOrder = berichtsheft.BerichtsheftOrder(requester.Group);

        string ans = "Die Aktuelle Reihenfolge ist:\n";

        foreach (var apprentice in berichtsheftWriterOrder.Item1)
        {
            ans += apprentice.Name;
            ans += "\n";
        }

        if (berichtsheftWriterOrder.Item2.Count > 0)
        {
            ans += "\nAzubis, die übersprungen werden:\n";
            foreach (var apprentice in berichtsheftWriterOrder.Item2)
            {
                ans += apprentice.Name;
                ans += "\n";
            }
        }

        await command.RespondAsync(ans);
    }
}