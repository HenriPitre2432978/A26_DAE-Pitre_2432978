using RobotFight.Models.Enums;

namespace RobotFight.Models
{
    public class RobotConfig(int hpPoints, int armorPoints, int damagePoints)
    {
        public int HpPoints { get; } = hpPoints;
        public int ArmorPoints { get; } = armorPoints;
        public int DamagePoints { get; } = damagePoints;

        public bool IsValid(int pointsToGive) =>
            HpPoints >= 0 && ArmorPoints >= 0 && DamagePoints >= 0 &&
            HpPoints + ArmorPoints + DamagePoints == pointsToGive;

        /// <summary>Lire la data string séparé par des ; pour les séparer et les assigner en stats</summary>
        public static RobotConfig? FromArgs(string[] args)
        {
            if (args.Length == 3 &&
                int.TryParse(args[0], out int hp) &&
                int.TryParse(args[1], out int armor) &&
                int.TryParse(args[2], out int damage))
                return new RobotConfig(hp, armor, damage);

            //Si invalide
            return null;
        }

        /// <summary>
        /// Get stats list of a robot
        /// </summary>
        public List<Stats> ToStats() =>
        [
            new Stats(StatsType.HP, Config.BASE_HP + HpPoints* Config.HP_PER_POINT),
            new Stats(StatsType.ARMOR, Config.BASE_ARMOR + ArmorPoints* Config.ARMOR_PER_POINT),
            new Stats(StatsType.DAMAGE, Config.BASE_DAMAGE + DamagePoints* Config.DAMAGE_PER_POINT),
            new Stats(StatsType.ENERGY, Config.BASE_ENERGY),
        ];


    };
}
