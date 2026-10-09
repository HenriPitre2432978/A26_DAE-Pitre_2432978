using RobotFight.Models;
using RobotFight.Models.Enums;

namespace RobotFight.Views
{
    /// <summary>
    /// Interface de format uniforme poour console view
    /// </summary>
    public interface IGameView
    {
        bool AskIsHostInstance();
        string AskIpAddress();
        int AskPort();
        void ShowMessage(string message, bool isSecondary = false, bool skipLines = false);
        void ShowWinner(Robot? robot); //null to prevent aucun gagnant (impossible masi on sait jamais)
        RobotConfig AskPlayerConfig(int pointsToGive);
        GameAction AskPlayerAction();
        bool AskPlayerReplay();
    }
}
