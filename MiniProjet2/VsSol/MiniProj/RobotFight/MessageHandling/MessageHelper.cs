using RobotFight.Models;
using RobotFight.Models.Enums;
using System.Text.Json;

namespace RobotFight.MessageHandling
{
    public static class MessageHelper
    {
        private static readonly JsonSerializerOptions JsonSerializerOption = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>
        /// Créer le message à partir des params donnés
        /// </summary>
        /// <param name="type">Type de message</param>
        /// <param name="action">Action reliée au message</param>
        /// <param name="status">Statu de la partie</param>
        /// <param name="data">Data liée au contexte du message</param>
        /// <returns>Le message formatté</returns>
        public static Message BuildMessage(MessageType type, GameAction action, GameStatus status, string data) =>
            new() { MessageType = type, Action = action, Status = status, Data = data ?? string.Empty };

        /// <summary>
        /// Créer le message à partir des params donnés
        /// </summary>
        /// <param name="type">Type de message</param>
        /// <param name="args">Paramètres liés au contexte du messages qui permettent de trouver les détails du message (action,data,etc.)</param>
        /// <returns>Le message formatté</returns>
        public static Message Build(MessageType type, params object[] args)
        {
            if (type == MessageType.ROBOT_CONFIG_OK && args.Length == 0)
                args = new object[] { CommandDictionary.OK };

            string data = string.Join(CommandDictionary.SEPARATOR, args);
            return BuildMessage(type, ExtractAction(type, data), StatusFor(type), data);
        }

        /// <summary>
        /// Écrire le message sous un foramt compréhensible en transfert
        /// </summary>
        /// <returns>La string du msg sérialisé</returns>
        public static string Serialize(Message message)
        {
            return JsonSerializer.Serialize(message, JsonSerializerOption);
        }

        /// <summary>
        /// Convertir un message formatté pour le transfert en format Message
        /// </summary>
        /// <param name="raw">String de text formatté d'un message</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Pas trouvé la conversion</exception>
        public static Message ParseMessage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                throw new FormatException("Message vide");

            //raw = raw.Trim();
            //MessageType type = CommandDictionary.GetMessageType(raw);
            //string[] parts = raw.Split(CommandDictionary.SEPARATOR, 2);
            //string data = parts.Length > 1 ? parts[1] : string.Empty;

            Message r = JsonSerializer.Deserialize<Message>(raw, JsonSerializerOption);

            //Créer le msg à partir des infos extracted
            return BuildMessage(r.MessageType, ExtractAction(r.MessageType, r.Data), StatusFor(r.MessageType), r.Data);
        }


        /// <summary>
        /// Tenter de convertir un message formatté pour le transfert en format Message
        /// </summary>
        /// <param name="raw">txt du message en format transfert</param>
        /// <param name="message">Nouveau message converti</param>
        /// <returns>true si réussite else false</returns>
        public static bool TryParseMessage(string raw, out Message? message)
        {
            try { message = ParseMessage(raw); return true; }
            catch (FormatException) { message = null; return false; }
        }

        /// <summary>
        /// Extraire le "ACTION" d'une raw string
        /// </summary>
        /// <param name="type">Le type de mesage de la raw string</param>
        /// <param name="data">raw data de la string</param>
        /// <returns>l'ACTION formatté en GameAction</returns>
        private static GameAction ExtractAction(MessageType type, string data)
        {
            string[] a = data.Split(CommandDictionary.SEPARATOR);
            string? value = type switch
            {
                MessageType.PLAYER_ACTION => a[0],
                MessageType.PLAYER_RESULT when a.Length > 1 => a[1],
                _ => null
            };
            return value != null && Enum.TryParse(value, true, out GameAction action) ? action : default;
        }

        /// <summary>
        /// Trouver l'équivalent de statut de partie pour chaque type de message
        /// </summary>
        /// <param name="type">Le type de message qui nécessite de savoir le statut</param>
        /// <returns>le status sous GameStatus</returns>
        private static GameStatus StatusFor(MessageType type) => type switch
        {
            MessageType.PLAYER_JOIN or MessageType.SERVER_BUSY
                => GameStatus.WAITING_FOR_PLAYER,

            MessageType.WELCOME or MessageType.ROBOT_CONFIG
                => GameStatus.WAITING_FOR_PLAYER_CONFIG,

            MessageType.ROBOT_CONFIG_OK
                => GameStatus.WAITING_FOR_HOST_CONFIG,

            MessageType.GAME_START or MessageType.TURN
            or MessageType.PLAYER_ACTION or MessageType.PLAYER_RESULT
                => GameStatus.PLAYING,

            //Other (dump)
            _ => GameStatus.END_GAME
        };
    }
}
