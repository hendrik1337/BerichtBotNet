using System.Globalization;

namespace BerichtsheftCreator.Data;

public class Lesson
{
    public string lesson { get; set; }
    public string Date { get; set; }
    public string Time { get; set; }
    public string FormText { get; set; }
    public string? NoteText { get; set; }
    public string Length { get; set; }

    public DateTime DateTime
    {
        get
        {
            string dateTimeString = $"{Date} {Time.Split("-")[0]}";
            return DateTime.ParseExact(dateTimeString, "d.M.yyyy HH:mm", CultureInfo.InvariantCulture);
        }
    }
}