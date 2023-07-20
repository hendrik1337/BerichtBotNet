using System.Globalization;
using BerichtBotNet.Data;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Helper;
using BerichtBotNet.Repositories;
using Discord.WebSocket;

namespace BerichtBotNet.Discord.Controller;

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

        string ans = "";

        if (logs.Count > 0)
        {
            ans += $"Die letzten {logs.Count} Log Einträge:\n\n";
            
            foreach (var log in logs)
            {
                ans += $"Berichtsheft: {log.BerichtheftNummer} " +
                       $"({WeekHelper.DateTimeToCalendarWeekYearCombination(log.Timestamp)}) " +
                       $"Azubi: {log.Apprentice.Name}" +
                       $"\n";
            }
        }
        else
        {
            ans += "Es sind keine Log Einträge vorhanden";
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

        BerichtsheftService berichtsheftService = new BerichtsheftService(_apprenticeRepository, _logRepository, _weeksRepository);
        
        try
        {
            var value = command.Data.Options.First().Options.First().Value;
            switch (command.Data.Options.First().Options.First().Name)
            {
                case "nummer":
                    await SendBerichtsheftWriterByNumber(command, value, berichtsheftService, requester);
                    break;
                case "datum":
                    await SendBerichtsheftWriterByDate(command, value, berichtsheftService, requester);
                    break;
            }
        }
        catch (InvalidOperationException ignored)
        {
            await command.RespondAsync(berichtsheftService.CurrentBerichtsheftWriterMessage(requester.Group, true));
        }


    }

    private static async Task SendBerichtsheftWriterByDate(SocketSlashCommand command, object value,
        BerichtsheftService berichtsheftService, Apprentice requester)
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
            Apprentice apprentice = berichtsheftService.GetBerichtsheftWriterOfDate(requester.Group, date);
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
        BerichtsheftService berichtsheftService, Apprentice requester)
    {
        var number = int.Parse(value.ToString()!);
        try
        {
            Apprentice apprentice = berichtsheftService.GetBerichtsheftWriterOfNumber(requester.Group, number);
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

        BerichtsheftService berichtsheftService = new BerichtsheftService(_apprenticeRepository, _logRepository, _weeksRepository);
        var berichtsheftWriterOrder = berichtsheftService.BerichtsheftOrder(requester.Group);

        string ans = "Die Aktuelle Reihenfolge ist:\n";

        foreach (var apprentice in berichtsheftWriterOrder.Item1)
        {
            ans += apprentice.Name;
            ans += "\n";
        }

        if (berichtsheftWriterOrder.Item2 is not null)
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