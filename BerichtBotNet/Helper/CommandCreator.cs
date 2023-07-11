﻿using System.Runtime.CompilerServices;
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
}