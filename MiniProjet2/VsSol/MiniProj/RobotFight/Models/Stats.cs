using RobotFight.Models.Enums;

namespace RobotFight.Models
{
    public class Stats
    {
        public StatsType Type { get; }
        public int BaseValue { get; }
        public int CurrentValue { get; private set; }

        public Stats(StatsType type, int baseValue)
        {
            Type = type;
            BaseValue = baseValue;
            CurrentValue = baseValue;
        }

        public void Add(int amount) => CurrentValue += amount;

        /// <summary>min = 0</summary>
        public void Subtract(int amount) => CurrentValue = Math.Max(0, CurrentValue - amount);

        /// <summary>Plafonne CurrentValue à max (utile pour énergie)</summary>
        public void Cap(int max) => CurrentValue = Math.Min(CurrentValue, max);

        public void Reset() => CurrentValue = BaseValue;

        public override string ToString() => CurrentValue.ToString();
    }
}
