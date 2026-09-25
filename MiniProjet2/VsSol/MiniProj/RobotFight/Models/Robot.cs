using RobotFight.Models.Enums;

namespace RobotFight.Models
{
    public class Robot(bool isHost, List<Stats> stats)
    {
        public bool IsHost { get; } = isHost;
        public bool IsRecharging { get; private set; }
        public bool IsDefending { get; private set; }

        private readonly Dictionary<StatsType, Stats> stats = stats.ToDictionary(s => s.Type);

        public int Hp => stats[StatsType.HP].CurrentValue;
        public int Energy => stats[StatsType.ENERGY].CurrentValue;

        /// <summary>Set all stats  à base value (début de partie et REPLAY).</summary>
        public void Reset()
        {
            foreach (Stats s in stats.Values) s.Reset();
            IsRecharging = false;
            IsDefending = false;
        }

        /// <summary>ATTAQUE : inflige les DEGATS de l'attaquant.</summary>
        public int Attack()
        {
            IsRecharging = false;
            return stats[StatsType.DAMAGE].CurrentValue;
        }

        /// <summary>
        /// Coût: 2 éne, Résultat: dég*2.
        /// DOIT ÊTRE GÉRÉ QUAND CALLED: Reprompter action si fail -1
        /// Renvoie -1 et si l'énergie est insuffisante, comme une action refusée.
        /// TODO: more robust (change diag et remove -1)
        /// </summary>
        public int AttackWithPower()
        {
            const int cost = 2;
            if (stats[StatsType.ENERGY].CurrentValue < cost) return -1;

            stats[StatsType.ENERGY].Subtract(cost);
            IsRecharging = false;
            return stats[StatsType.DAMAGE].CurrentValue * 2;
        }

        /// <summary>
        /// def: +5 d'armure sur la prochaine attaque reçue
        /// </summary>
        public void Defend()
        {
            IsRecharging = false;
            IsDefending = true;
        }

        /// <summary>
        /// énergie = min(énergie + 1, 5)
        /// </summary>
        public void Recharge()
        {
            stats[StatsType.ENERGY].Add(1);

            //Capper l'energie si valeur actuelle est plus haute que le max
            stats[StatsType.ENERGY].Cap(Config.MAX_ENERGY);

            IsRecharging = true;
        }

        /// <summary>
        /// degats reçu = max(1, attack - (ARMURE + bonus de défense de soi)).
        /// Le bonus de défense retombe à 0 dès qu'une attaque est subie.
        /// Renvoie les dégâts subis pour le message RESULT.
        /// </summary>
        public int ReceiveDamage(int incomingAttack)
        {
            int armor = stats[StatsType.ARMOR].CurrentValue + (IsDefending ? Config.DEFENSE_BONUS : 0);
            int damage = Math.Max(1, incomingAttack - armor);

            stats[StatsType.HP].Subtract(damage);
            IsDefending = false;
            return damage;
        }

        /// <summary>Valeur d'une stat pour l'afficher (TODO: remove car pas obligé mais suivre diag)</summary>
        public string GetStats(StatsType type) => stats[type].CurrentValue.ToString();
    }
}
