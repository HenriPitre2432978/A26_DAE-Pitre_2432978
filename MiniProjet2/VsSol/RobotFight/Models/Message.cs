using RobotFight.Models.Enums;
using System.Text.Json.Serialization;

namespace RobotFight.Models
{
    /// <summary>
    /// Objet "container" qui contient tout ce qui doit être transmis à l'autre côté.
    /// Séparer les DATA par des ";"
    /// </summary>
    public class Message
    {
        public MessageType Type { get; set; }

        [JsonIgnore]
        public GameAction Action { get; set; }

        /// <summary>
        /// ACTION JSON accepte action ou null si début/pas daction et le traduit en dequoi dacceptable
        /// </summary>
        [JsonPropertyName("action")]
        public GameAction? ActionJson
        {
            get => Action;
            set => Action = value ?? default;
        }

        [JsonIgnore]
        public GameStatus Status { get; set; }

        /// <summary>
        /// STATUS JSON accepte action ou null si début/pas de statut et le traduit en dequoi dacceptable
        /// </summary>
        [JsonPropertyName("status")]
        public GameStatus? StatusJson
        {
            get => Status;
            set => Status = value ?? default;
        }

        private string data = string.Empty;

        /// <summary>Toute la data parsed en string, séparé par ; . null = ""</summary>
        public string Data
        {
            get => data;
            set => data = value ?? string.Empty;
        }

        /// <summary>data séparé par ; get = data[0], data[1]...</summary>
        [JsonIgnore]
        public string[] Args => Data.Length == 0 ? [] : Data.Split(';');

        public override bool Equals(object? obj) =>
            obj is Message m && Type == m.Type && Action == m.Action
            && Status == m.Status && Data == m.Data;

        public override int GetHashCode() => HashCode.Combine(Type, Action, Status, Data);

        public override string ToString() => $"{Type} [{Data}]";
    }
}
