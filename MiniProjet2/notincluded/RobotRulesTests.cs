namespace RobotFight;

using RobotFight.Models;
using RobotFight.Models.Enums;

[TestClass]
public class RobotRulesTests
{
    private static Robot MakeRobot(int hp = 100, int armor = 0, int damage = 10, int energy = 2) =>
        new(true,
        [
            new Stats(StatsType.HP, hp),
            new Stats(StatsType.ARMOR, armor),
            new Stats(StatsType.DAMAGE, damage),
            new Stats(StatsType.ENERGY, energy),
        ]);

    [TestMethod]
    public void ForceStat_SetsHpAndEnergy()
    {
        Robot r = MakeRobot();
        r.ForceStat(42, 4);
        Assert.AreEqual(42, r.Hp);
        Assert.AreEqual(4, r.Energy);
    }

    [TestMethod]
    public void Recharge_Gives2EnergyCappedAtMax()
    {
        Robot r = MakeRobot(energy: 1);
        r.Recharge();
        Assert.AreEqual(3, r.Energy);

        r.ForceStat(100, 4);
        r.Recharge();
        Assert.AreEqual(Config.MAX_ENERGY, r.Energy);
    }

    [TestMethod]
    public void Repair_Costs1EnergyAndHeals()
    {
        Robot r = MakeRobot(energy: 2);
        r.ForceStat(50, 2);

        int healed = r.Repair();

        Assert.AreEqual(10, healed);       // max(5, 10% de 100)
        Assert.AreEqual(60, r.Hp);
        Assert.AreEqual(1, r.Energy);
    }

    [TestMethod]
    public void Repair_RefusedWithoutEnergyOrAtFullHp()
    {
        Robot r = MakeRobot();
        r.ForceStat(50, 0);
        Assert.AreEqual(-1, r.Repair());

        r.ForceStat(100, 3);
        Assert.AreEqual(-1, r.Repair());
        Assert.AreEqual(3, r.Energy);
    }

    [TestMethod]
    public void Defend_BonusIsMaxOfMinBonusAnd40PercentOfArmor()
    {
        // armure 20 -> bonus = max(5, 8) = 8 ; dégâts = 30 - (20 + 8) = 2
        Robot r = MakeRobot(armor: 20);
        r.Defend();
        Assert.AreEqual(2, r.ReceiveDamage(30));

        // armure 0 -> bonus = max(5, 0) = 5 ; dégâts = 10 - 5 = 5
        Robot r2 = MakeRobot(armor: 0);
        r2.Defend();
        Assert.AreEqual(5, r2.ReceiveDamage(10));
    }

    [TestMethod]
    public void Defend_EnergyRechargeHappensRoughly40PercentOfTheTime()
    {
        Randomize.SetSeed(1234);
        int recharged = 0;
        for (int i = 0; i < 1000; i++)
        {
            Robot r = MakeRobot(energy: 0);
            if (r.Defend()) recharged++;
        }
        Assert.IsTrue(recharged is > 330 and < 470, $"recharged = {recharged}");
    }
}
