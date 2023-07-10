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

    public static string DateTimetoCalendarWeek(DateTime date)
    {
        int calendarWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

        string calendarWeekString = "KW " + calendarWeek.ToString() + " " + date.Year.ToString();

        return calendarWeekString;
    }
}