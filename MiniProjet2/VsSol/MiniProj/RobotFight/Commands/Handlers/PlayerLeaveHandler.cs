using RobotFight.Connexion;
using RobotFight.Models;
using RobotFight.Views;

namespace RobotFight.Commands.Handlers
{
    /// <summary>
    /// Gère le connexion fail lorsque serveur est déjà occupé
    /// </summary>
    public class PlayerLeaveHandler(IMessageSender sender, IGameView view, Action onLeave) : CommandBase(sender, view)
    {
        private readonly Action onLeave = onLeave;

        /// <summary>
        /// Handle SERVER_BUSY : avertit client et ferme connexion
        /// </summary>
        public override Task Handle(Message message)
        {
            //un seul msg possible donc pas de verif
            view.ShowMessage("Le serveur est occupé, une partie est déjà en cours.");

            //Envoyer on leave pour callback dans gamecontroller
            onLeave();
            return Task.CompletedTask;
        }
    }
}
