namespace BerichtBotNet.Data;

public class SkippedWeeks
{
    public int Id { get; set; }
    // Is transformed into Calendar Week (KW 9 2023)
    private DateTime _skippedWeek;
    public DateTime SkippedWeek
    {
        get => _skippedWeek.ToLocalTime();
        set => _skippedWeek = value.ToUniversalTime();
    }
    public int GroupId { get; set; }
}