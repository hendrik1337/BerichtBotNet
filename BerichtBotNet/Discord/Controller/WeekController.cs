using BerichtBotNet.Data;
using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public class WeekController
{
    private readonly ApprenticeRepository _apprenticeRepository;
    private readonly SkippedWeeksRepository _weeksRepository;

    public WeekController(ApprenticeRepository apprenticeRepository, SkippedWeeksRepository weeksRepository)
    {
        _apprenticeRepository = apprenticeRepository;
        _weeksRepository = weeksRepository;
    }

    public void WeekCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Options.First().Name)
        {
            case "überspringen":
                SkipWeek(command);
                break;
            case "entfernen":
                RemoveWeek(command);
                break;
            case "anzeigen":
                ShowWeeks(command);
                break;
        }
    }

    private async Task<bool> ValidateRequest<T>(T command, Apprentice? requester)
    {
        if (requester is null)
        {
            if (command is SocketSlashCommand slashCommand)
                await slashCommand.RespondAsync(Constants.UserNotRegistered);
            else if (command is SocketMessageComponent messageComponent)
                await messageComponent.RespondAsync(Constants.UserNotRegistered);
            else if (command is SocketModal modal)
                await modal.RespondAsync(Constants.UserNotRegistered);

            return false;
        }

        return true;
    }

    public async void SkipWeek(SocketSlashCommand command)
    {
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        if (!ValidateRequest(command, requester).Result) return;

        var commandValue = command.Data.Options.First().Options.First().Value;
        var dates = commandValue.ToString()!.Trim().Split(",");

        try
        {
            var parsedDates = WeekHelper.StringArrayToDateTimeList(dates);

            foreach (var date in parsedDates)
            {
                SkippedWeeks week = new SkippedWeeks()
                {
                    SkippedWeek = date.ToUniversalTime(),
                    GroupId = requester.Group.Id
                };
                _weeksRepository.Create(week);
            }

            string ans = "Datum / Daten: ";

            foreach (var date in parsedDates)
            {
                ans += "\n";
                ans += date.ToString("d", Constants.CultureInfo);
                ans += " (";
                ans += WeekHelper.DateTimeToCalendarWeekYearCombination(date);
                ans += ") ";
            }

            ans += "\nhinzugefügt";

            await command.RespondAsync(ans);
        }
        catch (FormatException ignored)
        {
            await command.RespondAsync("Unbekanntes Datumsformat. Bitte in DD.MM.YYYY / DD.MM.YY angeben.");
        }
    }

    private async void RemoveWeek(SocketSlashCommand command)
    {
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        if (!ValidateRequest(command, requester).Result) return;

        var commandValue = command.Data.Options.First().Options.First().Value;
        var dates = commandValue.ToString()!.Trim().Split(",");

        try
        {
            var parsedDates = WeekHelper.StringArrayToDateTimeList(dates);
            var skippedWeeks = _weeksRepository.GetByGroupId(requester.Group.Id);


            var skippedWeeksToDelete = skippedWeeks
                .Where(skippedWeek => parsedDates
                    .Any(date =>
                        WeekHelper.DateTimeToCalendarWeekYearCombination(date)
                            .Equals(WeekHelper.DateTimeToCalendarWeekYearCombination(skippedWeek.SkippedWeek))))
                .ToList();

            foreach (var skippedWeek in skippedWeeksToDelete)
            {
                _weeksRepository.Delete(skippedWeek.Id);
            }

            string ans = "Datum / Daten: ";

            foreach (var date in skippedWeeksToDelete)
            {
                ans += "\n";
                ans += date.SkippedWeek.ToString("d", Constants.CultureInfo);
                ans += " (";
                ans += WeekHelper.DateTimeToCalendarWeekYearCombination(date.SkippedWeek);
                ans += ") ";
            }

            ans += "\ngelöscht";

            await command.RespondAsync(ans);
        }
        catch (FormatException ignored)
        {
            await command.RespondAsync("Unbekanntes Datumsformat. Bitte in DD.MM.YYYY / DD.MM.YY angeben.");
        }
    }
    
    private async void ShowWeeks(SocketSlashCommand command)
    {
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        if (!ValidateRequest(command, requester).Result) return;

        var skippedWeeks = _weeksRepository.GetByGroupId(requester.Group.Id);

        if (skippedWeeks.Count == 0)
        {
            await command.RespondAsync("Es werden keine Wochen übersprungen");
            return;
        }
        
        string ans = "Datum / Daten: ";

        foreach (var date in skippedWeeks)
        {
            ans += "\n";
            ans += date.SkippedWeek.ToString("d", Constants.CultureInfo);
            ans += " (";
            ans += WeekHelper.DateTimeToCalendarWeekYearCombination(date.SkippedWeek);
            ans += ") ";
        }

        ans += "\nwerden übersprungen";

        await command.RespondAsync(ans);
    }
}