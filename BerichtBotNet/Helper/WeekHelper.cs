using System.Globalization;

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
}