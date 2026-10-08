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

        /// <summary>
        /// Enlever qté amount de cette stats au robot
        /// </summary>
        /// <param name="amount">qte à enlever</param>
        public void Subtract(int amount) => CurrentValue = Math.Max(0, CurrentValue - amount);

        /// <summary>Cap la qté si current value est plus grosse que le max</summary>
        /// <param name="max">qté maximale à atteindre</param>
        public void Cap(int max) => CurrentValue = Math.Min(CurrentValue, max);


        /// <summary>
        /// Change la valeur de la stat
        /// </summary>
        /// <param name="value">nouvelle valeur de la stat</param>
        public void Set(int value) => CurrentValue = Math.Max(0, value);

        /// <summary>
        /// Reinitialiser la valeur de la stat à celle initiale (BaseValue)
        /// </summary>
        public void Reset() => CurrentValue = BaseValue;
    }
}
