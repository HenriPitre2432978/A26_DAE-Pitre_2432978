using RobotFight.Models.Enums;

namespace RobotFight.Models
{
    public class Game
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public GameStatus Status { get; private set; } = GameStatus.WAITING_FOR_PLAYER_CONFIG;

        /// <summary>
        /// 0 = au tour de l'hôte, 1 = au tour du client. 
        /// L'hôte joue toujours le premier tour donc default=0.
        /// </summary>
        public int CurrentPlayer { get; private set; } = 0;

        public List<Robot> Robots { get; }
        public Robot HostRobot => Robots[0];
        public Robot ClientRobot => Robots[1];

        /// <summary>
        /// Dégâts du dernier Play() réussi (0 pour def/reload).
        /// </summary>
        public int LastDamage { get; private set; }

        public Game(Robot hostRobot, Robot playerRobot)
        {
            Robots = new List<Robot> { hostRobot, playerRobot };
        }

        /// <summary>
        /// Initialize (reset) les robots et recommencer le tour à hote.
        /// </summary>
        public void StartGame()
        {
            foreach (Robot robot in Robots) robot.Reset();
            CurrentPlayer = 0;
            Status = GameStatus.PLAYING;
        }

        /// <summary>
        /// Vérifier si un des deux robots est à pv=0
        /// </summary>
        /// <returns>true si pv=0 pour un des deux</returns>
        public bool CheckGameEnded()
        {
            bool ended = HostRobot.Hp <= 0 || ClientRobot.Hp <= 0;
            if (ended) Status = GameStatus.END_GAME;
            return ended;
        }

        /// <summary>
        /// Check si pv=0, si oui check lequel est encore vivant
        /// </summary>
        /// <returns>Null si aucun gagnant, Sinon obj Robot gagnant</returns>
        public Robot? GetWinner()
        {
            if (!CheckGameEnded()) return null;
            if (HostRobot.Hp <= 0 && ClientRobot.Hp <= 0) return null; // case égal, supposément impossible puisque les deux sont lock.

            //Retourner celui ayant pv=0
            return HostRobot.Hp > 0 ? HostRobot : ClientRobot;
        }

        /// <summary>
        /// Applique une action pour le camp donné, avec exactement les mêmes règles quel que soit
        /// qui joue. Renvoie false (et ne change rien) si l'action est refusée — ex. ATTAQUE_PUISSANTE
        /// sans assez d'énergie — pour que l'appelant sache que le tour n'a PAS été consommé.
        /// </summary>
        public bool Play(bool isHostTurn, GameAction action)
        {
            Robot actor;
            Robot target;
            switch (isHostTurn)
            {
                case true:
                    actor = HostRobot;
                    target = ClientRobot;
                    break;
                case false:
                    actor = ClientRobot;
                    target = HostRobot;
                    break;
            }

            //reset last dmg pour afficher 0 si déf ou reload (au lieu de l'écraswer)
            LastDamage = 0;

            switch (action)
            {
                case GameAction.ATTACK:
                    LastDamage = target.ReceiveDamage(actor.Attack());
                    break;

                case GameAction.ATTACK_PUISSANCE:
                    int power = actor.AttackWithPower();

                    if (power < 0) return false;   // énergie insuffisante, play = false
                    LastDamage = target.ReceiveDamage(power);
                    break;

                case GameAction.DEFENSE:
                    actor.Defend();
                    break;

                case GameAction.RECHARGE:
                    //Déjà à 5; eviter waste turn
                    if (actor.Energy >= Config.MAX_ENERGY) return false;
                    actor.Recharge();
                    break;
            }

            CheckGameEnded();
            if (Status != GameStatus.END_GAME)
                CurrentPlayer = isHostTurn ? 1 : 0;

            //Si rendu jusque là, play = true
            return true;
        }
    }
}
