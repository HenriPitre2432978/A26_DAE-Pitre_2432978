using RobotFight.Models.Enums;

namespace RobotFight.MessageHandling
{
    /// <summary>
    /// Dictionnaire des messages possibles et de leur format de base (selon diag classe)
    /// </summary>
    public static class CommandDictionary
    {
        public const char SEPARATOR = ';';
        public const string OK = "OK";

        public const string CREATE = "CREATE";
        public const string JOIN = "JOIN";
        public const string WELCOME = "WELCOME;{nomHote}";
        public const string SERVER_BUSY = "SERVER_BUSY";
        public const string ROBOT = "ROBOT;{pv};{armure};{degats}";
        public const string ROBOT_OK = "ROBOT;OK";
        public const string ERROR = "ERROR;{code}";
        public const string START = "START;{pvHote};{pvClient};{energieHote};{energieClient}";
        public const string TURN = "TURN;{HOTE|CLIENT}";
        public const string ACTION = "ACTION;{action}";
        public const string RESULT = "RESULT;{HOTE|CLIENT};{action};{degats};{pvHote};{pvClient};{energieHote};{energieClient}";
        public const string END = "END;{HOTE|CLIENT};{pvHote};{pvClient}";
        public const string REPLAY = "REPLAY";
        public const string REPLAY_OK = "REPLAY;OK";
        public const string QUIT = "QUIT";
        public const string QUIT_OK = "QUIT;OK";

        /// <summary>
        /// Keywords utilisé pour converitr le msg en indication qui passe dans la connexion TCP
        /// </summary>
        private static readonly Dictionary<MessageType, string> Keywords = new()
        {
            [MessageType.PLAYER_JOIN] = "JOIN",
            [MessageType.WELCOME] = "WELCOME",
            [MessageType.SERVER_BUSY] = "SERVER_BUSY",
            [MessageType.ROBOT_CONFIG] = "ROBOT",
            [MessageType.ROBOT_CONFIG_OK] = "ROBOT",
            [MessageType.ERROR] = "ERROR",
            [MessageType.GAME_START] = "START",
            [MessageType.TURN] = "TURN",
            [MessageType.PLAYER_ACTION] = "ACTION",
            [MessageType.PLAYER_RESULT] = "RESULT",
            [MessageType.GAME_END] = "END",
            [MessageType.PLAYER_REPLAY] = "REPLAY",
            [MessageType.QUIT] = "QUIT",
        };
        /// <summary>
        /// Get Keyword command equivalent from a messagetype
        /// </summary>
        /// <returns>string of the keyword commands</returns>
        public static string GetCommand(MessageType type) => Keywords[type];

        /// <summary>
        /// Get the MessageType equivalent from a key word command
        /// </summary>
        /// <exception cref="FormatException">Pas trouvé d'équivalent</exception>
        public static MessageType GetMessageType(string command)
        {
            string[] parts = command.Split(SEPARATOR, 2);
            string keyword = parts[0].Trim().ToUpperInvariant();

            if (keyword == "ROBOT")
                return parts.Length > 1 && parts[1] == OK ? MessageType.ROBOT_CONFIG_OK : MessageType.ROBOT_CONFIG;

            foreach (var (type, kw) in Keywords)
                if (kw == keyword) return type;

            throw new FormatException($"Commande inconnue : {command}");
        }
    }
}
