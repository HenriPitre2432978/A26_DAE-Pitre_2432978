using System.Net;
using System.Net.Sockets;

namespace RobotFight.Models
{
    /// <summary>
    /// Default game settings (TODO: Ajuster diag)
    /// </summary>
    public class Config
    {
        public const int PORT = 6767;
        /// <summary>
        /// Get local ip to connect using UDP pour avoir la bonne IP, pas un gateway desactivé
        /// </summary>
        /// <returns>ip en string, ou "127.0.0.1" si aucun est dispo.</returns>
        public static string GetIp()
        {
            try
            {
                // get udp socket en faisant semblant d'envoyer un packet
                // "using = dump le socket quand fini, parce que inutile"
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                // fake send un packet pour force find la bonne ip communicable (le bon endpoint)
                socket.Connect("8.8.8.8", 8888);

                return ((IPEndPoint)socket.LocalEndPoint).Address.ToString();
            }
            catch
            {
                // si pas trouvé, return localhost error:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nAucune IP publique trouvée, avez-vous vérifier votre connexion Internet/VPN/Pare-Feu ? ");
                Console.ResetColor();

                return "127.0.0.1";
            }
        }

        public const int MAX_PLAYERS = 2;
        public const int POINTS_TO_GIVE = 10;
        public const int BASE_ENERGY = 2;
        public const int MAX_ENERGY = 5;
        public const int BASE_ARMOR = 0;
        public const int BASE_HP = 100;
        public const int BASE_DAMAGE = 10;
        public const int HP_PER_POINT = 10;
        public const int ARMOR_PER_POINT = 2;
        public const int DAMAGE_PER_POINT = 2;
        public const int DEFENSE_BONUS = 5;
        public const int DEFENSE_BONUS_PERCENT = 40;
        public const int DEFENSE_RECHARGE_CHANCE_PERCENT = 40;
        public const int DEFENSE_RECHARGE_AMOUNT = 1;
        public const int RECHARGE_AMOUNT = 2;
        public const int REPAIR_ENERGY_COST = 1;
        public const int REPAIR_PERCENT = 10;
        public const int REPAIR_MIN_HP = 5;
        public const int DODGE_ENERGY_COST = 1;
        public const int LUCK_TO_ESCAPE_PERCENT = 15;
        public const int MIN_SUCCESS_PERCENT = 50;
        public const int MAX_SUCCESS_PERCENT = 85;
        public const int MAX_FAIL_STREAK = 2;
    }
}
