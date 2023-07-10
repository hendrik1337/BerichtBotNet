namespace BerichtBotNet.Data;

public class SkippedWeeks
{
    public int Id { get; set; }
    // Is transformed into Calendar Week (CW 9 2023)
    public DateTime SkippedWeek { get; set; }
    public int GroupId { get; set; }
}