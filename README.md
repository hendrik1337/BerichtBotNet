# BerichtBotNet

### Ein Discord-Bot, der erinnert, wer diese Woche das Berichtsheft für die Berufsschule / Schulungen schreiben muss.

    Erinnerungen: Der Bot sendet automatisch Benachrichtigungen an die registrierten Benutzer, um sie an das Schreiben ihres Berichtshefts zu erinnern.
    Wochenplan: Der Bot berücksichtigt den Wochenplan der Auszubildenden und sendet die Erinnerungen entsprechend.
    Konfiguration: Die Erinnerungszeit und andere Einstellungen können über einfache Befehle konfiguriert werden.
    Benutzerfreundlichkeit: Der Bot bietet eine einfache Schnittstelle für die Registrierung, Konfiguration und Abmeldung von Erinnerungen.

### Installation
##### Direkt

    Installiere .NET Core.
    Erstelle eine PostgreSQL-Datenbank.
    Klone das Repository.
    Navigiere zum Verzeichnis des Projekts und führe den folgenden Befehl aus, um die erforderlichen Abhängigkeiten zu installieren:
    dotnet restore
    Setze die Umgebungsvariablen DiscordToken und PostgreSQLBerichtBotConnection.
    Starte den Bot mit dem Befehl dotnet run.

#### Docker

    Installiere Docker.
    Klone das Repository.
    Passe die Daten in der docker-compose.yaml an.
    Starte den Bot mit den Befehl docker-compose up -d.

### Verwendung

Um den Bot zu verwenden, registriere dich zunächst mit dem Befehl /azubi hinzufügen. Erstelle eine Gruppe und gebe dann deinen Discord-Namen und ID. Der Bot sendet dir dann automatisch Benachrichtigungen an die angegebene Zeit, um dich an das Schreiben deines Berichtshefts zu erinnern.

Du kannst die Erinnerungszeit und andere Einstellungen über die Befehle /gruppe bearbeiten konfigurieren.

Um dich von Erinnerungen abzumelden, kannst du den Befehl /azubi löschen verwenden.

#### Befehle

##### Der Bot unterstützt die folgenden Befehle:

    /azubi - Befehle zur Verwaltung von Azubis
    /gruppe - Befehle zur Verwaltung von Gruppen
    /berichtsheft - Befehle für die Berichtshefte
    /woche - Befehle zum Überspringen von Wochen
    /hilfe - Befehle zum Anzeigen der Anleitung

##### Befehle zur Verwaltung von Azubis

    /azubi hinzufügen - Fügt einen Azubi hinzu
    /azubi bearbeiten - Bearbeitet einen Azubi
    /azubi löschen - Entfernt einen Azubi
    /azubi überspringen - Überspringt einen Azubi
    /azubi überspringen-entfernen - Entfernt die Überspringung für einen Azubi

##### Befehle zur Verwaltung von Gruppen

    /gruppe hinzufügen - Fügt eine Gruppe hinzu
    /gruppe bearbeiten - Bearbeitet eine Gruppe
    /gruppe löschen - Entfernt eine Gruppe

##### Befehle für die Berichtshefte

    /berichtsheft wer - Zeigt Informationen zum Berichtsheft einer bestimmten Nummer an
    /berichtsheft reihenfolge - Zeigt die aktuelle Reihenfolge der Berichtsheftschreiber an
    /berichtsheft log - Zeigt vergangene Berichtsheftschreiber an

##### Befehle zum Überspringen von Wochen

    /woche überspringen - Überspringt eine oder mehrere Wochen
    /woche entfernen - Setzt das Überspringen einer Woche zurück
    /woche anzeigen - Zeigt die übersprungenen Wochen an

##### Befehle zum Anzeigen der Anleitung

    /hilfe befehl - Zeigt eine ausführliche Beschreibung für einen bestimmten Befehl an
    /hilfe befehle - Zeigt eine allgemeine Beschreibung der Befehle an
    /hilfe bug - Bug / Verbesserungsvorschläge

### Feedback

Bitte teile mir dein Feedback zu dem Bot mit, indem du eine Issue einreichst.

