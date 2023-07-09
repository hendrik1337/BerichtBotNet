using System.Runtime.CompilerServices;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;


namespace BerichtBotNet.Helper;

public class CommandCreator
{

    public static async Task CreateCommands(DiscordSocketClient client)
    {
        
    }

    private static async Task AzubiCommands(DiscordSocketClient client)
    {
        var globalAzubiCommand = new SlashCommandBuilder()
            .WithName("azubi")
            .WithDescription("Befehle um Azubis zu verwalten")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("befehl")
                .WithDescription("Welchen Befehl möchtest du ausführen?")
                .WithRequired(true)
                .AddChoice("hinzufügen", "add")
                .AddChoice("bearbeiten", "edit")
                .AddChoice("löschen", "remove")
                .WithType(ApplicationCommandOptionType.String)
            );

        try
        {
            await client.Rest.CreateGlobalCommand(globalAzubiCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private static async Task GroupCommands(DiscordSocketClient client)
    {
        var globalGroupCommand = new SlashCommandBuilder()
            .WithName("gruppe")
            .WithDescription("Befehle um Gruppen zu verwalten")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("befehl")
                .WithDescription("Welchen Befehl möchtest du ausführen?")
                .WithRequired(true)
                .AddChoice("hinzufügen", "add")
                .AddChoice("bearbeiten", "edit")
                .AddChoice("löschen", "remove")
                .WithType(ApplicationCommandOptionType.String)
            );

        try
        {
            await client.Rest.CreateGlobalCommand(globalGroupCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private static async Task BerichtsheftCommands(DiscordSocketClient client)
    {
        var globalGroupCommand = new SlashCommandBuilder()
            .WithName("wer")
            .WithDescription("Gibt den Berichtsheftschreiber zurück")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("nummer")
                .WithDescription("Welche Berichtsheftnummer? (optional)")
                .WithRequired(false)
                .AddOption("nummer", ApplicationCommandOptionType.String, "",
                    isRequired: true)
                .WithType(ApplicationCommandOptionType.String)
            );

        try
        {
            await client.Rest.CreateGlobalCommand(globalGroupCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private static async Task WeekCommands(DiscordSocketClient client)
    {
        var globalAzubiCommand = new SlashCommandBuilder()
            .WithName("woche")
            .WithDescription("Kommandos um Wochen auszulassen.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("überspringen")
                .WithDescription("Woche(n) überspringen")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("value", ApplicationCommandOptionType.String, "Datum / Daten z.B. 08.07.2023, ...",
                    isRequired: true)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("entfernen")
                .WithDescription("Überspringen einer Woche zurücksetzen")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("value", ApplicationCommandOptionType.String, "Datum / Daten z.B. 08.07.2023, ...",
                    isRequired: true)
            ).AddOption(new SlashCommandOptionBuilder()
                .WithName("anzeigen")
                .WithDescription("Wochen, die übersprungen werden anzeigen.")
                .WithType(ApplicationCommandOptionType.SubCommand)
            );

        try
        {
            await client.Rest.CreateGlobalCommand(globalAzubiCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}