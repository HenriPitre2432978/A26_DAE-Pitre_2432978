using RobotFight.Models;
using RobotFight.Models.Enums;
using Base = BibliothequeFonctionsDeBase.FonctionsDeBase;

namespace RobotFight.Views
{
    /// <summary>
    /// La seule classe qui devrait communiquer directement avec la console
    /// </summary>
    public class ConsoleView : IGameView
    {
        public static void BaseDisplay(string title) => Base.AfficherTitre(title);

        public bool AskIsHostInstance()
        {
            BaseDisplay("CHOIX DE L'INSTANCE");

            bool isHost;
            Base.DisplayLineChoices(["Créer la partie", "Rejoindre une partie en attente"]);

            if (Base.LireChiffre("# Choix: ", 2).ToString()[0] == '1')
                isHost = true;
            else isHost = false;

            return isHost;
        }

        public string AskIpAddress() => Base.LireAdresseIP("Ip du Serveur : ");

        public int AskPort()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            int r = Base.LireEntierMinMax("Port : ", 1024, 49151);
            Console.ResetColor();
            return r;
        }

        public void ShowMessage(string message, bool isSecondary = false, bool skipLines = false)
        {
            ConsoleColor c = ConsoleColor.White;
            if (isSecondary)
            {
                c = ConsoleColor.DarkGray;
            }
            if (skipLines)
            {
                Console.WriteLine();
                Base.AfficherTexte(message, c);
                Console.WriteLine();
                Console.WriteLine();
            }
            else
                Base.AfficherTexteCentre(message);

            Console.ResetColor();
        }
        public void ShowWinner(Robot? robot)
        {
            if (robot == null)
            {
                Base.AfficherTexteCentre("Égalité : les deux robots sont tombés à 0 PV.");
                return;
            }
            Base.AfficherTexteCentre(robot.IsHost ? "L'hôte remporte le combat !" : "Le client remporte le combat !");
        }

        public RobotConfig AskPlayerConfig(int pointsToGive)
        {
            while (true)
            {
                Base.AfficherTexteCentre($"Répartissez {pointsToGive} points.");
                RobotConfig conf = new(Base.LireEntierMinMax("  PV     : ", 0, 10), Base.LireEntierMinMax("  ARMURE : ", 0, 10), Base.LireEntierMinMax("  DÉGÂTS : ", 0, 10));
                if (conf.IsValid(pointsToGive)) return conf;
                Base.AfficherTexteCentre($"Le total doit faire exactement {pointsToGive}.");
            }
        }

        public GameAction AskPlayerAction()
        {
            ShowMessage("Action — (A)ttaque, (D)éfense, (P)uissance, (R)echarge : ", true, true);
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
            //Clear le buffer pour pas queue la prochaine action
            Base.FlushInput();

            ConsoleKey key;
            do key = Console.ReadKey(true).Key;
            while (!allowed.Contains(key));
            return key;
        }
    }
}
