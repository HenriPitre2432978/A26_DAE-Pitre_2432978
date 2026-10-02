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
                ConsoleView.BaseDisplay("CONFIGURATION DU ROBOT");

                view.ShowMessage("");
                view.ShowMessage("===========================================");
                view.ShowMessage("Nouvelle partie : Reconfigurez votre robot.");
                view.ShowMessage("===========================================\n");
                if (onReplayOk != null) await onReplayOk();
                return;
            }

            //Replay reçu, on confirme et avertit controller
            view.ShowMessage("");
            view.ShowMessage("=======================");
            view.ShowMessage("Le joueur veut rejouer.");
            view.ShowMessage("=======================\n");

            Thread.Sleep(1000);
            await Reply(MessageType.PLAYER_REPLAY, CommandDictionary.OK);
            onReplay?.Invoke();
        }
    }
}
