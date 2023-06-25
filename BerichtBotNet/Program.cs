// See https://aka.ms/new-console-template for more information

using BerichtBotNet.Discord;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace BerichtBotNet;

class BerichtBotNet
{
    private DiscordSocketClient _client = null!;

    public static Task Main(string[] args) => new BerichtBotNet().MainAsync();

    public async Task MainAsync()
    {
        _client = new DiscordSocketClient();

        _client.Log += Log;

        _client.SlashCommandExecuted += SlashCommandHandler;

        _client.Ready += Client_Ready;

        _client.ModalSubmitted += ModalSubmittedHandler;

        var token = Environment.GetEnvironmentVariable("DiscordToken");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }

    private async Task Client_Ready()
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
            await _client.Rest.CreateGlobalCommand(globalAzubiCommand.Build());
        }
        catch (HttpException exception)
        {
            var json = JsonConvert.SerializeObject(exception.ToString(), Formatting.Indented);
            Console.WriteLine(json);
        }
        
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

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "azubi":
                ApprenticeCommands.ApprenticeCommandHandler(command);
                break;
            case "gruppe":
                GroupCommands.GroupCommandHandler(command);
                break;
        }
    }

    private async Task ModalSubmittedHandler(SocketModal modal)
    {
        if (modal.Data.CustomId.Contains("ApprenticeMenu"))
        {
            ApprenticeCommands.ApprenticeModalHandler(modal);
        }
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}