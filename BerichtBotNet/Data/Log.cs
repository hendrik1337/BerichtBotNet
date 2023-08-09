namespace BerichtBotNet.Data;

/// <summary>
/// Class which defines which values are saved in the logging table
/// </summary>
public class Log
{
    public int Id { get; set; }
    public virtual Apprentice Apprentice { get; set; }
    public int BerichtheftNummer { get; set; }
    
    private DateTime _timestamp;
    public DateTime Timestamp
    {
        get => _timestamp.ToLocalTime();
        set => _timestamp = value.ToUniversalTime();
    }

    public virtual Group Group { get; set; } = null!;
}