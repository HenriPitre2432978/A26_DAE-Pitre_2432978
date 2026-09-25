using RobotFight.Models;
using RobotFight.Models.Enums;

namespace RobotFight.Views
{
    /// <summary>
    /// La seule classe qui devrait communiquer directement avec la console
    /// </summary>
    public class ConsoleView : IGameView
    {
        public string AskPlayerType()
        {
            Console.Write("Serveur (S) ou client (C) ? ");
            ConsoleKey key = ReadKey(ConsoleKey.S, ConsoleKey.C);
            return key == ConsoleKey.S ? "S" : "C";
        }

        public string AskIpAddress()
        {
            Console.Write("IP du serveur : ");
            string? ip = Console.ReadLine();
            return string.IsNullOrWhiteSpace(ip) ? Config.IP_ADDRESS : ip.Trim();
        }
        public int AskPort()
        {
            int port = AskInt("Port : ");
            return port;
        }

        public void ShowMessage(string message) => Console.WriteLine(message);

        public void ShowWinner(Robot? robot)
        {
            if (robot == null)
            {
                Console.WriteLine("Égalité : les deux robots sont tombés à 0 PV.");
                return;
            }
            Console.WriteLine(robot.IsHost ? "L'hôte remporte le combat !" : "Le client remporte le combat !");
        }

        public RobotConfig AskPlayerConfig(int pointsToGive)
        {
            while (true)
            {
                Console.WriteLine($"Répartissez {pointsToGive} points.");
                RobotConfig conf = new(AskInt("  PV     : "), AskInt("  Armure : "), AskInt("  Dégâts : "));
                if (conf.IsValid(pointsToGive)) return conf;
                Console.WriteLine($"Le total doit faire exactement {pointsToGive}.");
            }
        }

        public GameAction AskPlayerAction()
        {
            Console.Write("Action — (A)ttaque, (D)éfense, (P)uissance, (R)echarge : ");
            return ReadKey(ConsoleKey.A, ConsoleKey.D, ConsoleKey.P, ConsoleKey.R) switch
            {
                ConsoleKey.D => GameAction.DEFENSE,
                ConsoleKey.P => GameAction.ATTACK_PUISSANCE,
                ConsoleKey.R => GameAction.RECHARGE,
                _ => GameAction.ATTACK
            };
        }

        public bool AskPlayerReplay()
        {
            Console.Write("Rejouer ? (O/N) ");
            return ReadKey(ConsoleKey.O, ConsoleKey.N) == ConsoleKey.O;
        }

        private static ConsoleKey ReadKey(params ConsoleKey[] allowed)
        {
            ConsoleKey key;
            do key = Console.ReadKey(true).Key;
            while (!allowed.Contains(key));
            Console.WriteLine(key);
            return key;
        }

        private static int AskInt(string prompt)
        {
            int value;
            do Console.Write(prompt);
            while (!int.TryParse(Console.ReadLine(), out value));
            return value;
        }
    }
}
