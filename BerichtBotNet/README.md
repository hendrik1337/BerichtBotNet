## Discord Bot: Berichtsheft-Erinnerung

Dies ist ein Discord-Bot, der in C# entwickelt wurde, um Azubis dabei zu unterstützen, sich an das Schreiben ihres Berichtshefts in einer bestimmten Woche zu erinnern. Der Bot sendet Benachrichtigungen an die Benutzer und hilft dabei, den Prozess der Berichtsheftführung zu vereinfachen.
Features

    Erinnerungen: Der Bot sendet automatisch Benachrichtigungen an die registrierten Benutzer, um sie an das Schreiben ihres Berichtshefts zu erinnern.
    Wochenplan: Der Bot berücksichtigt den Wochenplan der Azubis und sendet die Erinnerungen entsprechend.
    Konfiguration: Die Erinnerungszeit und andere Einstellungen können über einfache Befehle konfiguriert werden.
    Benutzerfreundlich: Der Bot bietet eine einfache Schnittstelle für die Registrierung, Konfiguration und Abmeldung von Erinnerungen.

### Installation
##### Direkt
    Installiere .NET Core.
    Erstelle eine PostgreSQL Datenbank
    Klonen das Repository .
    Navigieren zum Verzeichnis des Projekts und führen den folgenden Befehl aus, um die erforderlichen Abhängigkeiten zu installieren:
    Setze den 'DiscordToken' und 'PostgreSQLBerichtBotConnection' in den Umgebungsvariablen    
    Führe 'dotnet restore' aus um Abhängigkeiten zu laden
    Starte den Bot mit 'dotnet run'

##### Docker
...