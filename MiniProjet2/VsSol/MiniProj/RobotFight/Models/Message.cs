using RobotFight.Models.Enums;

namespace RobotFight.Models
{
    /// <summary>
    /// Objet "container" qui contient tout ce qui doit être transmis à l'autre côté.
    /// Séparer les DATA par des ";"
    /// </summary>
    public class Message
    {
        public MessageType Type { get; set; }
        public GameAction Action { get; set; }
        public GameStatus Status { get; set; }

        /// <summary>Toute la data parsed en string, séparé par ;</summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>Data split on ';' → data[0], data[1]...</summary>
        public string[] Args => Data.Length == 0 ? Array.Empty<string>() : Data.Split(';');

        public override string ToString() => $"{Type} [{Data}]";
    }
}
