using System.Globalization;
using BerichtBotNet.Exceptions;

namespace BerichtBotNet.Helper;

public class WeekHelper
{
    public static List<DateTime> StringArrayToDateTimeList(string[] stringDates)
    {
        List<DateTime> parsedList = new List<DateTime>();
        foreach (string date in stringDates)
        {
            parsedList.Add(DateTime.Parse(date, Constants.CultureInfo));
        }

        return parsedList;
    }

    public static string DateTimeToCalendarWeekYearCombination(DateTime date)
    {
        return "KW " + ISOWeek.GetWeekOfYear(date) + " " + ISOWeek.GetYear(date);;
    }

    public static int GetBerichtsheftNumber(DateTime ausbildungsStart, DateTime currentDate)
    {
        if (currentDate.DayOfWeek == DayOfWeek.Sunday)
        {
            currentDate = currentDate.AddDays(-6);
        }
        
        // Setzt beide Tage auf Montag und entfernt die Uhrzeit, um nur die Wochen zu unterscheiden
        currentDate = currentDate.Date.AddDays(-(int)currentDate.DayOfWeek + 1);
        ausbildungsStart = ausbildungsStart.Date.AddDays(-(int)ausbildungsStart.DayOfWeek + 1);
        
        

        TimeSpan duration = currentDate.Subtract(ausbildungsStart);
        
        // + 1, weil die Nummerierung bei 1 und nicht bei 0 beginnt
        return (int)Math.Ceiling(duration.TotalDays / 7) + 1;
    }

    public static DayOfWeek ParseStringIntoDayOfWeek(string dayString)
    {
        dayString = dayString.ToLower();
        switch (dayString)
        {
            case "mo":
                return DayOfWeek.Monday;
            case "di":
                return DayOfWeek.Wednesday;
            case "mi":
                return DayOfWeek.Wednesday;
            case "do":
                return DayOfWeek.Thursday;
            case "fr":
                return DayOfWeek.Friday;
            case "sa":
                return DayOfWeek.Saturday;
            case "so":
                return DayOfWeek.Sunday;
            default:
                throw new InvalidWeekDayInputException();
        }
    }
    
    public static string ParseDayOfWeekIntoString(DayOfWeek day)
    {
        switch (day)
        {
            case DayOfWeek.Monday:
                return "Mo";
            case DayOfWeek.Tuesday:
                return "Di";
            case DayOfWeek.Wednesday:
                return "Mi";
            case DayOfWeek.Thursday:
                return "Do";
            case DayOfWeek.Friday:
                return "Fr";
            case DayOfWeek.Saturday:
                return "Sa";
            case DayOfWeek.Sunday:
                return "So";
            default:
                throw new ArgumentException("Invalid DayOfWeek value.");
        }
    }
}