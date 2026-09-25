using RobotFight.Connexion;
using RobotFight.MessageHandling;
using RobotFight.Models.Enums;
using RobotFight.Models;
using RobotFight.Views;

namespace RobotFight.Commands.Handlers
{
    /// <summary>
    /// Gère la demande de replay et sa confirmation (REPLAY;OK)
    /// </summary>
    public class PlayerReplayHandler(IMessageSender sender, IGameView view, Action? onReplay = null, Func<Task>? onReplayOk = null) : CommandBase(sender, view)
    {
        private readonly Action? onReplay = onReplay;
        private readonly Func<Task>? onReplayOk = onReplayOk;

        /// <summary>
        /// Handle la demande de revanche et sa confirmation
        /// </summary>
        /// <param name="message">Le message de replay ou d'acceptation</param>
        public override async Task Handle(Message message)
        {
            if (message.Data == CommandDictionary.OK)
            {
                //replay confirmé serv-side: refaire OnWelcome
                view.ShowMessage("Nouvelle partie : reconfigurez votre robot.");
                if (onReplayOk != null) await onReplayOk();
                return;
            }

            //Replay reçu, on confirme et avertit controller
            view.ShowMessage("Le joueur veut rejouer.");
            await Reply(MessageType.PLAYER_REPLAY, CommandDictionary.OK);
            onReplay?.Invoke();
        }
    }
}
