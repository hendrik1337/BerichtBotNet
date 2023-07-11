using BerichtBotNet.Data;
using BerichtBotNet.Exceptions;
using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public class BerichtsheftController
{
    private readonly ApprenticeRepository _apprenticeRepository;
    private readonly SkippedWeeksRepository _weeksRepository;
    private readonly LogRepository _logRepository;

    public BerichtsheftController(ApprenticeRepository apprenticeRepository, SkippedWeeksRepository weeksRepository,
        LogRepository logRepository)
    {
        _apprenticeRepository = apprenticeRepository;
        _weeksRepository = weeksRepository;
        _logRepository = logRepository;
    }

    public void BerichtsheftCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "wer":
                SendCurrentBerichtsheftWriter(command);
                break;
        }
    }

    private async void SendCurrentBerichtsheftWriter(SocketSlashCommand command)
    {
        Apprentice? requester = _apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());

        if (requester == null)
        {
            await command.RespondAsync(
                $"Du wurdest nicht in der Datenbank gefunden. Registriere dich mit '/azubi hinzufügen'");
            return;
        }

        Berichtsheft berichtsheft = new Berichtsheft(_apprenticeRepository, _logRepository, _weeksRepository);
        
        await command.RespondAsync(berichtsheft.CurrentBerichtsheftWriterMessage(requester.Group));
    }
}