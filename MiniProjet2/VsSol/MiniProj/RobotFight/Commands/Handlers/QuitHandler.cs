using RobotFight.Connexion;
using RobotFight.MessageHandling;
using RobotFight.Models;
using RobotFight.Models.Enums;
using RobotFight.Views;

namespace RobotFight.Commands.Handlers
{
    /// <summary>
    /// Gère la fermeture de la partie QUIT et sa confirmation QUIT;OK server et clkient side
    /// </summary>
    public class QuitHandler(IMessageSender sender, IGameView view, Action onQuit) : CommandBase(sender, view)
    {
        //Callback à utiliser dans gamecontreoller
        private readonly Action onQuit = onQuit;

        /// <summary>
        /// Handle la demande de quitter et sa confirmation
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override async Task Handle(Message message)
        {
            if (message.Data != CommandDictionary.OK)
            {
                //On reçoit QUIT et confirme (send) avant de fermer 
                view.ShowMessage("L'autre joueur a quitté la partie.");
                await Reply(MessageType.QUIT, CommandDictionary.OK);
            }

            //meme si pas ok, on ferme
            onQuit();
        }
    }
}
