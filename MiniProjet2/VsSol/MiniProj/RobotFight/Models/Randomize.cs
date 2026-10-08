namespace RobotFight.Models
{
    /// <summary>
    /// Tout l'aléatoire du jeu. Utilisé seulement par le Game pour éviter de call côté client et alter les prob
    /// </summary>
    public static class Randomize
    {
        private static Random random = new();

        /// <summary>Fixer seed (tests?)</summary>
        public static void SetSeed(int seed) => random = new Random(seed);

        /// <summary>
        /// Décide oui/non selon pourcentage chance du roll
        /// </summary>
        /// <param name="percentChance">Pourcentage de réussite</param>
        /// <returns>true si réussi, sinon false (non/pas réussi)</returns>
        public static bool Roll(int percentChance) => random.Next(100) < percentChance;

        /// <summary>
        /// Décide si la défense recharge de l'É (random)
        /// </summary>
        /// <returns>true si oui, else false</returns>
        public static bool HasDefenseRecharge() => Roll(Config.DEFENSE_RECHARGE_CHANCE_PERCENT);

        /// <summary>
        /// Réussite d'une action risquée: la chance va de MIN_SUCCESS_PERCENT (PV à 0)
        /// à MAX_SUCCESS_PERCENT (PV pleins), proportionnellement aux PV restants.
        /// </summary>
        public static bool HasCompleteRandom(int hpBase, int hpNow)
        {
            double ratio = hpBase <= 0 ? 0 : Math.Clamp((double)hpNow / hpBase, 0, 1);
            int chance = Config.MIN_SUCCESS_PERCENT
                + (int)Math.Round((Config.MAX_SUCCESS_PERCENT - Config.MIN_SUCCESS_PERCENT) * ratio);
            return Roll(chance);
        }

        /// <summary>Réussite de la fuite (LUCK_TO_ESCAPE_PERCENT %)</summary>
        public static bool RandomEscape() => Roll(Config.LUCK_TO_ESCAPE_PERCENT);
    }
}
