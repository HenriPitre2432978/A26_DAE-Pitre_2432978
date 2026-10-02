using System.Net;

namespace RobotFight.Models
{
    /// <summary>
    /// Default game settings (TODO: Ajuster diag)
    /// </summary>
    public class Config
    {
        public const int PORT = 6767;
        public static string GetIp()
        {
            try
            {
                var ip = Dns.GetHostAddresses(Dns.GetHostName())
                    .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                                         && !IPAddress.IsLoopback(a));
                return ip?.ToString() ?? "127.0.0.1";
            }
            catch { return "127.0.0.1"; }
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
    }
}
