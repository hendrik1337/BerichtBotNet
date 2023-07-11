using BerichtBotNet.Data;
using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public static class WeekCommands
{
    public static void WeekCommandHandler(SocketSlashCommand command)
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

    private static async void skipWeek(SocketSlashCommand command)
    {
        var commandValue = command.Data.Options.First().Options.First().Value;
        var dates = commandValue.ToString()!.Trim().Split(",");

        try
        {
            var parsedDates = WeekHelper.StringArrayToDateTimeList(dates);
            await using BerichtBotContext context = new BerichtBotContext();
            SkippedWeeksRepository skippedWeeksRepository = new SkippedWeeksRepository(context);
            ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
            Apprentice? user = apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());

            if (user == null)
            {
                await command.RespondAsync("Du wurdest nicht in der DB gefunden. Bitte Registriere dich zuerst");
                return;
            }
            
            foreach (var date in parsedDates)
            {
                SkippedWeeks week = new SkippedWeeks()
                {
                    SkippedWeek = date.ToUniversalTime(),
                    GroupId = user.Group.Id
                };
                skippedWeeksRepository.Create(week);
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