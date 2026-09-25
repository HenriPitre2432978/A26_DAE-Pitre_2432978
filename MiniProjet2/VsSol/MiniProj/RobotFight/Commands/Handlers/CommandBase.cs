using RobotFight.Connexion;
using RobotFight.MessageHandling;
using RobotFight.Models;
using RobotFight.Models.Enums;
using RobotFight.Views;

namespace RobotFight.Commands.Handlers
{
    /// <summary>Commun à chaque handler, contiennent leur propre logique.</summary>
    public abstract class CommandBase : ICommand
    {
        protected readonly IMessageSender sender;
        protected readonly IGameView view;

        protected CommandBase(IMessageSender sender, IGameView view)
        {
            this.sender = sender;
            this.view = view;
        }

        public abstract Task Handle(Message message);

        /// <summary>Construit et envoie un message de réponse du type donné.</summary>
        protected Task Reply(MessageType type, params object[] args) =>
            sender.Send(MessageHelper.Build(type, args));
    }
}
