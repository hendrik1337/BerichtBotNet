namespace BerichtBotNet.Data;

/// <summary>
/// Class which defines how an Apprentice (Azubi) is saved in the Database
/// </summary>
public class Apprentice
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string DiscordUserId { get; set; } = null!;
    public virtual Group Group { get; set; } = null!;
    public bool Skipped { get; set; }
}