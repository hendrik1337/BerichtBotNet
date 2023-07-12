namespace BerichtBotNet.Helper;

using System.Globalization;

public sealed class Constants
{
    public static readonly CultureInfo CultureInfo = new CultureInfo("de-DE");
    public static readonly string? ServerId = System.Environment.GetEnvironmentVariable("ServerID");
    public static readonly string UserNotRegistered =
        "Du wurdest nicht in der Datenbank gefunden. Registriere dich mit '/azubi hinzufügen'";
}