using System.Runtime.CompilerServices;
using System.Text;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;


namespace BerichtBotNet.Helper;

public class CommandCreator
{
    private readonly DiscordSocketClient _client;

    public CommandCreator(DiscordSocketClient client)
    {
        _client = client;
    }

    public async Task CreateCommands()
    {
        // await WeekCommands();
        // await AzubiCommands();
        // await GroupCommands();
        // await BerichtsheftCommands();
        // await HelpCommands();
    }

    private async Task AzubiCommands()
    {
        Console.WriteLine("Creating Azubi Commands");
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
                .AddChoice("überspringen", "skip")
                .AddChoice("überspringen-entfernen", "un-skip")
                .WithType(ApplicationCommandOptionType.String)
            );

        try
        {
            await _client.Rest.CreateGlobalCommand(globalAzubiCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private async Task GroupCommands()
    {
        Console.WriteLine("Creating Group Commands");
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
            await _client.Rest.CreateGlobalCommand(globalGroupCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private async Task BerichtsheftCommands()
    {
        Console.WriteLine("Creating Berichtsheft Commands");

        var globalGroupCommand = new SlashCommandBuilder()
            .WithName("berichtsheft")
            .WithDescription("Befehle für die Berichtshefte.")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("wer")
                .WithDescription("Welche Berichtsheftnummer? (optional)")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithRequired(false)
                .AddOption("nummer", ApplicationCommandOptionType.Integer,
                    "Gibt an wer jetzt oder in der Vergangenheit das Berichtsheft geschrieben hat.",
                    isRequired: false)
                .AddOption("datum", ApplicationCommandOptionType.String,
                    "Gibt an wer jetzt oder in der Vergangenheit das Berichtsheft geschrieben hat.",
                    isRequired: false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("reihenfolge")
                .WithDescription("Gibt die Aktuelle Reihenfolge der Berichtsheftschreiber zurück")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithRequired(false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("log")
                .WithDescription("Gibt die vergangenen Berichtsheftschreiber zurück")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption("anzahl", ApplicationCommandOptionType.Integer,
                    "Wie viele Logs sollen angezeigt werden (default=10)", isRequired: false)
                .WithRequired(false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("generieren")
                .WithDescription("Generiert ein Berichtsheft für die aktuelle Woche")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithRequired(false));

        try
        {
            await _client.Rest.CreateGlobalCommand(globalGroupCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private async Task WeekCommands()
    {
        Console.WriteLine("Creating Week Commands");
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
            await _client.Rest.CreateGlobalCommand(globalAzubiCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
    }

    private async Task HelpCommands()
    {
        Console.WriteLine("Creating Help Commands");

        var globalAzubiCommand = new SlashCommandBuilder()
            .WithName("hilfe")
            .WithDescription("Zeigt die Liste der verfügbaren Befehle an")
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("befehl")
                .WithDescription("Zeigt eine ausführliche Beschreibung für einen bestimmten Befehl an")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithRequired(false)
                .AddOption("name", ApplicationCommandOptionType.String, "Der Name des Befehls", isRequired: false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("befehle")
                .WithDescription("Zeigt eine allgemeine Beschreibung der Befehle an")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithRequired(false))
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("bug")
                .WithDescription("Bug / Verbesserungsvorschläge")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .WithRequired(false));

        try
        {
            await _client.Rest.CreateGlobalCommand(globalAzubiCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}