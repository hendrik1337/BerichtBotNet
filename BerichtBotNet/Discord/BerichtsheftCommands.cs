using BerichtBotNet.Data;
using BerichtBotNet.Helper;
using BerichtBotNet.Models;
using BerichtBotNet.Repositories;
using Discord.WebSocket;

namespace BerichtBotNet.Discord;

public class BerichtsheftCommands
{
    public static void BerichtsheftCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "wer":
                SendCurrentBerichtsheftWriter(command);
                break;
        }
    }

    private static void SendCurrentBerichtsheftWriter(SocketSlashCommand command)
    {
        using BerichtBotContext context = new BerichtBotContext();
        ApprenticeRepository apprenticeRepository = new ApprenticeRepository(context);
        Apprentice? requester = apprenticeRepository.GetApprenticeByDiscordId(command.User.Id.ToString());
        Apprentice currentBerichtsheftWriter = Berichtsheft.GetCurrentBerichtsheftWriterOfGroup(requester.Group.Id);

        command.RespondAsync($"Azubi: {currentBerichtsheftWriter.Name} ist diese Woche dran.");
    }
}