namespace BerichtBotNet.Data;

/// <summary>
/// Class which defines a Group for the BerichtBot
/// </summary>
public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? DiscordGroupId { get; set; }
}