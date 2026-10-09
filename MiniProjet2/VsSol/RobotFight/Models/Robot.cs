using RobotFight.Models.Enums;

namespace RobotFight.Models
{
    public class Robot(bool isHost, List<Stats> stats)
    {
        public bool IsHost { get; } = isHost;
        public bool IsRecharging { get; private set; }
        public bool IsDefending { get; private set; }
        public bool IsDodging { get; private set; }
        private int failStreak;

        private readonly Dictionary<StatsType, Stats> stats = stats.ToDictionary(s => s.Type);

        public int Hp => stats[StatsType.HP].CurrentValue;
        public bool IsAlive() => Hp > 0;
        public int Energy => stats[StatsType.ENERGY].CurrentValue;

        /// <summary>Set all stats  à base value (début de partie et REPLAY).</summary>
        public void Reset()
        {
            foreach (Stats s in stats.Values) s.Reset();
            IsRecharging = false;
            IsDefending = false;
            IsDodging = false;
            failStreak = 0;
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
        /// Défense: sur prochaine attaque:
        /// bonus d'armure = Max(DEFENSE_BONUS, armure * DEFENSE_BONUS_PERCENT %).
        /// aussi DEFENSE_RECHARGE_CHANCE_PERCENT % de chance
        /// que points d'énergie += DEFENSE_RECHARGE_AMOUNT 
        /// </summary>
        /// <returns>true si É rechargée</returns>
        public bool Defend()
        {
            IsRecharging = false;
            IsDefending = true;
            IsDodging = false;

            if (!Randomize.HasDefenseRecharge()) return false;

            stats[StatsType.ENERGY].Add(Config.DEFENSE_RECHARGE_AMOUNT);
            stats[StatsType.ENERGY].Cap(Config.MAX_ENERGY);
            return true;
        }

        /// <summary>
        /// É = min(É + RECHARGE_AMOUNT, MAX_ENERGY)
        /// </summary>
        public void Recharge()
        {
            stats[StatsType.ENERGY].Add(Config.RECHARGE_AMOUNT);

            //Capper l'energie si valeur actuelle est plus haute que le max
            stats[StatsType.ENERGY].Cap(Config.MAX_ENERGY);

            IsRecharging = true;
        }




        /// <summary>
        /// ACTION pour soigner les pv
        /// Max(REPAIR_MIN_HP, PV de base * REPAIR_PERCENT %)
        /// Coute REPAIR ENERY COST points d'é pour réparer.
        /// </summary>
        /// <returns>-1 si fail, sinon nb pv soignés.</returns>
        public int Repair()
        {
            Stats hpStat = stats[StatsType.HP];

            //fail cases
            if (stats[StatsType.ENERGY].CurrentValue < Config.REPAIR_ENERGY_COST) return -1;
            if (hpStat.CurrentValue >= hpStat.BaseValue) return -1;

            //success donc spend energy
            stats[StatsType.ENERGY].Subtract(Config.REPAIR_ENERGY_COST);
            IsRecharging = false;

            int qte = Math.Max(Config.REPAIR_MIN_HP, hpStat.BaseValue * Config.REPAIR_PERCENT / 100);
            int healed = Math.Min(qte, hpStat.BaseValue - hpStat.CurrentValue);

            hpStat.Add(healed);
            return healed;
        }

        public bool CanDodge => Energy >= Config.DODGE_ENERGY_COST;

        /// <summary>
        /// coute DODGE_ENERGY_COST énergie si CanDodge.
        /// Si réussit, prock att annulée (0 dmg). depend du random(selon PV),
        /// garantie après MAX_FAIL_STREAK échecs de suite.
        /// </summary>
        /// <returns>true si l'esquive est réussie</returns>
        public bool Dodge()
        {
            stats[StatsType.ENERGY].Subtract(Config.DODGE_ENERGY_COST);
            IsRecharging = false;
            IsDefending = false;
            IsDodging = HasCompleted();
            return IsDodging;
        }

        /// <summary>
        /// s'échapper et terminer la partie en match nul
        /// LUCK_TO_ESCAPE_PERCENT % chance success
        /// </summary>
        public bool Escape()
        {
            IsRecharging = false;
            IsDefending = false;
            IsDodging = false;
            return Randomize.RandomEscape();
        }

        /// <summary>Roll completion selon random; garanti success apres config failstreak MAX atteinte</summary>
        private bool HasCompleted()
        {
            Stats hpStat = stats[StatsType.HP];
            bool success = failStreak >= Config.MAX_FAIL_STREAK
                || Randomize.HasCompleteRandom(hpStat.BaseValue, hpStat.CurrentValue);

            failStreak = success ? 0 : failStreak + 1;
            return success;
        }

        /// <summary>
        /// Calc et administre les dégats d'une attaque
        /// max(1, attack - (ARMURE + bonus de défense de soi))
        /// </summary>
        /// <param name="incomingAttack">ENUM NB de l'attaque</param>
        /// <returns>qte dommages recus</returns>
        public int ReceiveDamage(int incomingAttack)
        {
            //si esquivé auparavant: attaque = 0
            if (IsDodging)
            {
                IsDodging = false;
                IsDefending = false;
                return 0;
            }

            int armor = stats[StatsType.ARMOR].CurrentValue;

            int defBonus = IsDefending
                ? Math.Max(Config.DEFENSE_BONUS, armor * Config.DEFENSE_BONUS_PERCENT / 100)
                : 0;

            //calc qte degat
            int result = Math.Max(1, incomingAttack - (armor + defBonus));

            //Administreer degats
            stats[StatsType.HP].Subtract(result);
            IsDefending = false;
            return result;
        }

        /// <summary>Valeur d'une stat pour l'afficher (TODO: remove car pas obligé mais suivre diag)</summary>
        public string GetStats(StatsType type) => stats[type].CurrentValue.ToString();
    }
}
