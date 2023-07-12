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
                skipWeek(command);
                break;
            case "entfernen":
                Console.WriteLine("Entfernen");
                break;
            case "anzeigen":
                Console.WriteLine("Anzeigen");
                break;
        }
    }

    public async void skipWeek(SocketSlashCommand command)
    {
        var commandValue = command.Data.Options.First().Options.First().Value;
        var dates = commandValue.ToString()!.Trim().Split(",");

        try
        {
            var parsedDates = WeekHelper.StringArrayToDateTimeList(dates);
            Apprentice? user = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());

            if (user == null)
            {
                await command.RespondAsync(Constants.UserNotRegistered);
                return;
            }
            
            foreach (var date in parsedDates)
            {
                SkippedWeeks week = new SkippedWeeks()
                {
                    SkippedWeek = date.ToUniversalTime(),
                    GroupId = user.Group.Id
                };
                _weeksRepository.Create(week);
            }

            string ans = "Datum / Daten: ";

            foreach (var date in parsedDates)
            {
                ans += date.ToString("d", Constants.CultureInfo);
                ans += " (";
                ans += WeekHelper.DateTimeToCalendarWeekYearCombination(date);
                ans += ") ";
            }

            ans += "hinzugefügt";

            await command.RespondAsync(ans);
        }
        catch (FormatException ignored)
        {
            await command.RespondAsync("Unbekanntes Datumsformat. Bitte in DD.MM.YYYY / DD.MM.YY angeben.");
        }


    }
}