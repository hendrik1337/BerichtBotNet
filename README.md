# BerichtBotNet

**BerichtBotNet** ist ein Discord-Bot zur Verwaltung von Berufsschul-Berichtsheften für Auszubildende.

Der Bot hilft dabei zu organisieren, welcher Azubi in einer Gruppe für eine bestimmte Woche das Berichtsheft für die Berufsschule schreiben muss.

## Funktionen

1. **Azubi-Verwaltung**
   - Hinzufügen, Bearbeiten und Löschen von Auszubildenden
   - Überspringen von Auszubildenden in der Berichtsheft-Reihenfolge für die Berufsschule

2. **Gruppen-Verwaltung**
   - Erstellen, Bearbeiten und Löschen von Ausbildungsgruppen
   - Festlegen von Erinnerungszeiten für Gruppen bezüglich der Berichtsheft-Erstellung

3. **Berufsschul-Berichtsheft-Funktionen**
   - Anzeigen des aktuellen Berichtsheft-Schreibers für die Berufsschule
   - Anzeigen der Reihenfolge für das Schreiben der Berufsschul-Berichtshefte

4. **Wochen-Verwaltung**
   - Überspringen von Wochen (z.B. Ferienzeiten)
   - Entfernen von übersprungenen Wochen
   - Anzeigen von übersprungenen Wochen

5. **Hilfe-Funktionen**
   - Anzeigen von verfügbaren Befehlen
   - Detaillierte Beschreibungen einzelner Befehle
   - Möglichkeit, Bugs zu melden oder Verbesserungsvorschläge einzureichen

## Befehle

Der Bot verwendet Slash-Befehle für alle Funktionen. Hier sind die Hauptbefehle:

- `/azubi`: Verwaltung von Auszubildenden in der Berichtsheft-Rotation
- `/gruppe`: Verwaltung von Ausbildungsgruppen
- `/berichtsheft`: Funktionen für Berufsschul-Berichtshefte
- `/woche`: Verwaltung von übersprungenen Wochen (z.B. Ferienzeiten)
- `/hilfe`: Anzeigen von Hilfeinformationen

Für detaillierte Informationen zu jedem Befehl kann `/hilfe befehl [Befehlsname]` verwendet werden.

## Technische Details

- **Programmiersprache**: C#
- **Framework**: Discord.NET
- **Datenbank**: PostgreSQL mit Entity Framework Core
- **Task-Scheduling**: Quartz.NET

## Installation und Einrichtung

1. Klonen des Repositories
2. Sicherstellen, dass .NET 6.0 oder höher installiert ist
3. Hinzufügen der erforderlichen Umgebungsvariablen:
   - `DiscordToken`: Discord Bot-Token
   - `PostgreSQLBerichtBotConnection`: Verbindungsstring für die PostgreSQL-Datenbank
4. Ausführen von `dotnet restore` zur Installation der Abhängigkeiten
5. Starten des Bots mit `dotnet run`
