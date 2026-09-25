using RobotFight.Connexion;
using RobotFight.Models;
using RobotFight.Models.Enums;
using RobotFight.Views;

namespace RobotFight.Commands.Handlers
{
    /// <summary>
    /// Gère le tour de jeu (TURN) et l'envoi/réception de l'action du joueur (PLAYER_ACTION).
    /// </summary>
    public class PlayerActionHandler(IMessageSender sender, IGameView view,
                                Func<GameAction, Task>? onAction = null) : CommandBase(sender, view)
    {
        private readonly Func<GameAction, Task>? onAction = onAction;

        /// <summary>
        /// Handle la notification de tour et la réception de l'action choisie
        /// </summary>
        public override async Task Handle(Message message)
        {
            if (message.Type == MessageType.TURN)
            {
                if (message.Data != "CLIENT")
                {
                    //Pas le tour du client: skip/wait
                    view.ShowMessage("Tour de l'hôte...");
                    return;
                }

                //tour client: on demande l'action au joueur et on le send
                await Reply(MessageType.PLAYER_ACTION, view.AskPlayerAction());
            }
            //Si on reçoit directement une action
            else if (message.Type == MessageType.PLAYER_ACTION && onAction != null)
            {
                //Renvoyer l'action reçue au server
                await onAction(message.Action);
            }
        }
    }
}
