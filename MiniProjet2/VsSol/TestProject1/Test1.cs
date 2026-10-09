namespace RobotFight;

using RobotFight.Models;
using RobotFight.MessageHandling;
using RobotFight.Models.Enums;

[TestClass]
public class MessageHelperTest
{

    [TestMethod]

    public void ShouldBuildActionAttackMessage()

    {

        var expected = "{\"type\":8,\"action\":0,\"status\":3,\"data\":\"Test data\"}";

        var actual = MessageHelper.BuildMessage(MessageType.PLAYER_ACTION, GameAction.ATTACK, GameStatus.PLAYING, "Test data");
        var messageString = MessageHelper.Serialize(actual);
        Assert.AreEqual(expected, messageString);

    }

    [TestMethod]

    public void ShouldParseActionAttackMessage()

    {

        var expected = new Message
        {

            Type = MessageType.PLAYER_ACTION,

            Action = GameAction.ATTACK,

            Data = "Test data",

            Status = GameStatus.PLAYING

        };

        var actual = MessageHelper.ParseMessage("""
            
            {"type": 8,"action": 0,"data": "Test data","status": 3}            
            """);

        Assert.AreEqual(expected.Type, actual.Type);
        Assert.AreEqual(expected.Action, actual.Action);
        Assert.AreEqual(expected.Status, actual.Status);
        Assert.AreEqual(expected.Data, actual.Data);

    }

}
