namespace BerichtBotNet.Data;
/// <summary>
/// Class which defines which values are saved in the logging table
/// </summary>
public class Log
{
    public int Id { get; set; }
    public virtual Apprentice Apprentice { get; set; } = null!;
    public int BerichtheftNummer { get; set; }
    public DateTime Timestamp { get; set; }
}