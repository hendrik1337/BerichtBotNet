namespace BerichtBotNet.Data;

/// <summary>
/// Class which defines a Group for the BerichtBot
/// </summary>
public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? DiscordGroupId { get; set; }
    public DateTime StartOfApprenticeship { get; set; }
    // Only Hour and Minute of ReminderTime is used. Date is ignored
    public DateTime ReminderTime { get; set; }
    public DayOfWeek ReminderWeekDay { get; set; }
}