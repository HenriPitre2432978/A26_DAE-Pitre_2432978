namespace RobotTests;

using RobotFight.MessageHandling;
using RobotFight.Models.Enums;
using RobotFight.Models;

[TestClass]
public class MessageHelperTest
{
    [TestMethod]
    public void ShouldBuildActionAttackMessage()
    {
        var expected = "{\"messageType\":8,\"action\":0,\"status\":3,\"data\":\"Test data\"}";

        Message actual = MessageHelper.BuildMessage(MessageType.PLAYER_ACTION, GameAction.ATTACK, GameStatus.PLAYING, "Test data");

        Console.WriteLine(MessageHelper.Serialize(actual));

        Assert.AreEqual(expected, MessageHelper.Serialize(actual));
    }

    [TestMethod]
    public void ShouldParseActionAttackMessage()
    {
        var expected = new Message
        {
            MessageType = MessageType.PLAYER_ACTION,
            Action = GameAction.ATTACK,
            Data = "Test data",
            Status = GameStatus.PLAYING
        };
        var actual = MessageHelper.ParseMessage("""
            
            {"messageType": 8,"action": 0,"data": "Test data","status": 3}            
            
            """);

        Assert.AreEqual(expected, actual);
    }
}