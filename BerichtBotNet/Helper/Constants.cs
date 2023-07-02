namespace BerichtBotNet.Helper;

using System.Globalization;

public sealed class Constants
{
    public static CultureInfo CultureInfo = new CultureInfo("de-DE");
    public static string? ServerId = System.Environment.GetEnvironmentVariable("ServerID");
}