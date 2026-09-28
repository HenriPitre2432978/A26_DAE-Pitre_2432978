using RobotFight.Connexion;
using RobotFight.Models;
using RobotFight.Models.Enums;
using RobotFight.Views;

namespace RobotFight.Commands.Handlers
{
    /// <summary>
    /// Gère la validité et annoncement de RobotConfig, d'Erreur et d'Énergie insuffisante
    /// </summary>
    public class RobotReadyHandler(IMessageSender sender, IGameView view,
                                Func<RobotConfig, Task>? onPlayerReady = null,
                                Func<Task>? onActionRefused = null) : CommandBase(sender, view)
    {
        private readonly Func<RobotConfig, Task>? onPlayerReady = onPlayerReady;
        private readonly Func<Task>? onActionRefused = onActionRefused;

        /// <summary>
        /// Handle Message Content validity and result showing
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override async Task Handle(Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.ROBOT_CONFIG:
                    RobotConfig? robot = RobotConfig.FromArgs(message.Args);

                    if (robot == null || !robot.IsValid(Config.POINTS_TO_GIVE))
                    {
                        //Send back message indicating config error
                        await Reply(MessageType.ERROR, "CONFIG");
                        return;
                    }

                    //Send back message indicating config ok
                    await Reply(MessageType.ROBOT_CONFIG_OK);

                    if (onPlayerReady != null) await onPlayerReady(robot);
                    break;

                case MessageType.ROBOT_CONFIG_OK:
                    view.ShowMessage("Robot accepté. En attente de l'adversaire...");
                    break;

                case MessageType.ERROR when message.Data == "CONFIG":

                    //Retry config if error
                    view.ShowMessage("Configuration refusée. Veuillez Recommencer.");
                    RobotConfig retry = view.AskPlayerConfig(Config.POINTS_TO_GIVE);

                    //Send back new message with new config
                    await Reply(MessageType.ROBOT_CONFIG, retry.HpPoints, retry.ArmorPoints, retry.DamagePoints);
                    break;

                //Gère les erreurs spécifiques à ACTION
                case MessageType.ERROR when message.Data == "ACTION":

                    //Un seul cas de fail possible:
                    view.ShowMessage("Énergie insuffisante : choisissez une autre action.");

                    //Callback asynchrone pour relancer le choix d'action
                    if (onActionRefused != null) await onActionRefused();
                    break;

                //Erreur générale
                case MessageType.ERROR:
                    view.ShowMessage($"Erreur : {message.Data}");
                    break;
            }
        }
    }
}
