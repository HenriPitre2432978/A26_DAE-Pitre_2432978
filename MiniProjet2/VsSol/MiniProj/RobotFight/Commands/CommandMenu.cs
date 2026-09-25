using RobotFight.Models;
using RobotFight.Models.Enums;

namespace RobotFight.Commands
{
    /// <summary>
    /// Créer format uniforme obligatoire des handlers ( Handle(etc) selon diag)
    /// </summary>
    public interface ICommand { Task Handle(Message message); }

    public class CommandMenu
    {

        /// <summary>
        /// Instancier dictionnaire des handlers(qui associent le handling avec le message associé)
        /// </summary>
        private readonly Dictionary<MessageType, ICommand> handlers = new();

        /// <summary>
        /// Ajoute un handler pour un, ou plusieurs types de messages
        /// </summary>
        /// <param name="handler">Handler qui gèrera les messages "types"</param>
        /// <param name="types">Tous les messages correspondants au handler "handler", séparés d'une virgule (param)</param>
        public void AddHandler(ICommand handler, params MessageType[] types)
        {
            //bind
            foreach (MessageType type in types)
                handlers[type] = handler;
        }

        /// <summary>
        /// Dispatch l'éxécution du bon handler lorsque son message est reçu en paramètre
        /// </summary>
        /// <param name="message">Le message qui lance l'éxéc</param>
        /// <returns></returns>
        public Task Execute(Message message) =>
            handlers.TryGetValue(message.Type, out ICommand? handler)
                ? handler.Handle(message)
                : Task.CompletedTask;  //Skip si pas de handler (unexpected) TODO: More robust
    }
}
