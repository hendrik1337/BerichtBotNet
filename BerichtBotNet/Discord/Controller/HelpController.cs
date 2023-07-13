using System.Text;
using Discord.WebSocket;

namespace BerichtBotNet.Discord.Controller;

public class HelpController
{
    public async void HelpCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Options.FirstOrDefault().Name)
        {
            case "befehle":
                SendGeneralHelper(command);
                break;
            case "befehl":
                SendBefehlHelper(command);
                break;
            case "bug":
                await command.RespondAsync(
                    "Du hast einen Bug gefunden oder hast einen Verbesserungsvorschlag?\nDann erstelle ein Issue auf https://github.com/hendrik1337/BerichtBotNet/issues");
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
        commandList.AppendLine("- /woche: Befehle zum Überspringen von Wochen");
        commandList.AppendLine("- /hilfe: Befehl zum anzeigen dieses Textes ");

        await command.RespondAsync(commandList.ToString());
    }

    private async void SendBefehlHelper(SocketSlashCommand command)
    {
        var commandName = command.Data.Options.FirstOrDefault().Options.FirstOrDefault().Value.ToString();
        if (!string.IsNullOrEmpty(commandName))
        {
            // Hier kannst du eine ausführliche Beschreibung für jeden Befehl hinzufügen
            switch (commandName.ToLower())
            {
                case "azubi":
                    await command.RespondAsync("Befehle zur Verwaltung von Azubis:\n\n" +
                                               "- /azubi hinzufügen: Fügt einen Azubi hinzu\n" +
                                               "- /azubi bearbeiten: Bearbeitet einen Azubi\n" +
                                               "- /azubi löschen: Entfernt einen Azubi\n" +
                                               "- /azubi überspringen: Überspringt einen Azubi\n" +
                                               "- /azubi überspringen-entfernen: Entfernt die Überspringung für einen Azubi");
                    break;
                case "gruppe":
                    await command.RespondAsync("Befehle zur Verwaltung von Gruppen:\n\n" +
                                               "- /gruppe hinzufügen: Fügt eine Gruppe hinzu\n" +
                                               "- /gruppe bearbeiten: Bearbeitet eine Gruppe\n" +
                                               "- /gruppe löschen: Entfernt eine Gruppe");
                    break;
                case "berichtsheft":
                    await command.RespondAsync("Befehle für die Berichtshefte:\n\n" +
                                               "- /berichtsheft wer: Zeigt Informationen zum Berichtsheft einer bestimmten Nummer an\n" +
                                               "- /berichtsheft reihenfolge: Zeigt die aktuelle Reihenfolge der Berichtsheftschreiber an\n" +
                                               "- /berichtsheft log: Zeigt vergangene Berichtsheftschreiber an");
                    break;
                case "woche":
                    await command.RespondAsync("Befehle zum Überspringen von Wochen:\n\n" +
                                               "- /woche überspringen: Überspringt eine oder mehrere Wochen\n" +
                                               "- /woche entfernen: Setzt das Überspringen einer Woche zurück\n" +
                                               "- /woche anzeigen: Zeigt die übersprungenen Wochen an");
                    break;
                case "hilfe":
                    await command.RespondAsync("Befehle zum Anzeigen der Anleitung:\n\n" +
                                               "- /hilfe befehl: Zeigt eine ausführliche Beschreibung für einen bestimmten Befehl an\n" +
                                               "- /hilfe befehle: Zeigt eine allgemeine Beschreibung der Befehle an\n" +
                                               "- /hilfe bug: Bug / Verbesserungsvorschläge");
                    break;
                default:
                    await command.RespondAsync("Unbekannter Befehl. Bitte gib einen gültigen Befehl an.");
                    break;
            }
        }
    }
}