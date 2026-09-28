using RobotFight.Connexion;
using RobotFight.Models;
using RobotFight.Models.Enums;
using RobotFight.Views;

namespace RobotFight.Commands.Handlers
{
    /// <summary>
    /// Gère l'affichage du début, du déroulement et de la fin de partie (clientside).
    /// </summary>
    public class PlayerResultHandler(IMessageSender sender, IGameView view) : CommandBase(sender, view)
    {

        /// <summary>
        /// Handle l'affichage de START / RESULT / END, et prompt le replay à la fin
        /// </summary>
        public override async Task Handle(Message message)
        {
            string[] a = message.Args;

            switch (message.MessageType)
            {
                case MessageType.GAME_START:     // diag: pvHote;pvClient;energieHote;energieClient
                    //Affiche les stats de départ des deux robots
                    view.ShowMessage($"Début ! Hôte {a[0]} PV / {a[2]} én. — Vous {a[1]} PV / {a[3]} én.");
                    break;

                case MessageType.PLAYER_RESULT:  // diag: qui(client/hote);action;degats;pvHote;pvClient;energieHote;energieClient
                    //Affiche le résultat du tour qui vient d'être joué
                    view.ShowMessage($"---");
                    view.ShowMessage($"{a[0]} joue {a[1]} ! ({a[2]} dégâts)");
                    view.ShowMessage($"Hôte {a[3]} PV, Vous {a[4]} PV");
                    view.ShowMessage($"---\n");
                    break;

                case MessageType.GAME_END:       // gagnant;pvHote;pvClient
                    //Annonce résultat selon le side et send msg demander replay ou quitter
                    view.ShowMessage(a[0] == "CLIENT" ? "Vous avez gagné !" : "Vous avez perdu.");
                    await Reply(view.AskPlayerReplay() ? MessageType.PLAYER_REPLAY : MessageType.QUIT);
                    break;
            }
        }
    }
}
