using System.Text;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public class HelpController
{
    public async void HelpCommandHandler(SocketSlashCommand command)
    {
        await command.Channel.SendMessageAsync(
            "Du hast einen Bug gefunden oder hast einen Verbesserungsvorschlag?\nDann erstelle ein Issue auf https://github.com/hendrik1337/BerichtBotNet/issues");
        switch (command.Data.Options.FirstOrDefault().Value)
        {
            case null:
                SendGeneralHelper(command);
                break;
            case "befehl":
                SendBefehlHelper(command);
                break;
        }
    }

    private async void SendGeneralHelper(SocketSlashCommand command)
    {
        var commandList = new StringBuilder();
        commandList.AppendLine("Verfügbare Befehle:");
        commandList.AppendLine("- /azubi: Befehle zur Verwaltung von Azubis");
        commandList.AppendLine("- /gruppe: Befehle zur Verwaltung von Gruppen");
        commandList.AppendLine("- /berichtsheft: Befehle für die Berichtshefte");
        commandList.AppendLine("- /woche: Kommandos zum Überspringen von Wochen");

        await command.RespondAsync(commandList.ToString());
    }

    private async void SendBefehlHelper(SocketSlashCommand command)
    {
        var commandName = command.Data.Options.FirstOrDefault(option => option.Name == "name")?.Value.ToString();
        if (!string.IsNullOrEmpty(commandName))
        {
            // Hier kannst du eine ausführliche Beschreibung für jeden Befehl hinzufügen
            switch (commandName.ToLower())
            {
                case "azubi":
                    await command.RespondAsync("Befehle zur Verwaltung von Azubis:\n\n" +
                                               "- `/azubi hinzufügen: Fügt einen Azubi hinzu\n" +
                                               "- `/azubi bearbeiten: Bearbeitet einen Azubi\n" +
                                               "- `/azubi löschen: Entfernt einen Azubi\n" +
                                               "- `/azubi überspringen: Überspringt einen Azubi\n" +
                                               "- `/azubi überspringen-entfernen: Entfernt die Überspringung für einen Azubi");
                    break;
                case "gruppe":
                    await command.RespondAsync("Befehle zur Verwaltung von Gruppen:\n\n" +
                                               "- `/gruppe hinzufügen: Fügt eine Gruppe hinzu\n" +
                                               "- `/gruppe bearbeiten: Bearbeitet eine Gruppe\n" +
                                               "- `/gruppe löschen: Entfernt eine Gruppe");
                    break;
                case "berichtsheft":
                    await command.RespondAsync("Befehle für die Berichtshefte:\n\n" +
                                               "- `/berichtsheft wer`: Zeigt Informationen zum Berichtsheft einer bestimmten Nummer an\n" +
                                               "- `/berichtsheft reihenfolge`: Zeigt die aktuelle Reihenfolge der Berichtsheftschreiber an\n" +
                                               "- `/berichtsheft log`: Zeigt vergangene Berichtsheftschreiber an");
                    break;
                case "woche":
                    await command.RespondAsync("Kommandos zum Überspringen von Wochen:\n\n" +
                                               "- `/woche überspringen`: Überspringt eine oder mehrere Wochen\n" +
                                               "- `/woche entfernen`: Setzt das Überspringen einer Woche zurück\n" +
                                               "- `/woche anzeigen`: Zeigt die übersprungenen Wochen an");
                    break;
                default:
                    await command.RespondAsync("Unbekannter Befehl. Bitte gib einen gültigen Befehl an.");
                    break;
            }
        }
    }
}