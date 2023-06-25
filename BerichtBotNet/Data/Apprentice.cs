namespace BerichtBotNet.Data;

/// <summary>
/// Class which defines how an Apprentice (Azubi) is saved in the Database
/// </summary>
public class Apprentice
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int DiscordUserId { get; set; }
    public Group Group { get; set; }
    public int SkipCount { get; set; }
}