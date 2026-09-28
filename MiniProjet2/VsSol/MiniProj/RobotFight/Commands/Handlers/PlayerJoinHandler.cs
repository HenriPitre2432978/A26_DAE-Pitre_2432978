using RobotFight.Connexion;
using RobotFight.Models;
using RobotFight.Models.Enums;
using RobotFight.Views;

namespace RobotFight.Commands.Handlers
{
    /// <summary>
    /// Gère l'arrivée client côté serveur (JOIN) et confirmation connexion client (affichage)(WELCOME).
    /// </summary>
    public class PlayerJoinHandler(IMessageSender sender, IGameView view, string hostName = "Hote",
                                Action? onHostJoined = null, Func<Task>? onWelcome = null) : CommandBase(sender, view)
    {
        private readonly string hostName = hostName;
        private readonly Action? onHostJoined = onHostJoined;
        private readonly Func<Task>? onWelcome = onWelcome;

        /// <summary>
        /// Handle la connexion d'un joueur (serveur) et l'accueil reçu (client)
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override async Task Handle(Message message)
        {
            if (message.MessageType == MessageType.PLAYER_JOIN)
            {
                //lorsque joueur connecté, avertir controller et showMsg to server
                view.ShowMessage("Un joueur s'est connecté.");
                await Reply(MessageType.WELCOME, hostName);

                //Dans gamecontroller, appelle prompthostconfig
                onHostJoined?.Invoke();
            }
            else if (message.MessageType == MessageType.WELCOME)
            {
                //Server acceuille client: call back à OnWelcome comme si premier join
                view.ShowMessage($"Connecté à {message.Data}.");

                //Indiquer action lorsque onWelcome est reçu (dans addhandler gamecontroller)
                if (onWelcome != null) await onWelcome();
            }
        }
    }
}
