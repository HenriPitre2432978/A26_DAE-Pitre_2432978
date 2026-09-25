using System;
using System.Collections.Generic;
using System.Text;

namespace RobotFight.Models.Enums
{
    public enum GameStatus
    {
        WAITING_FOR_PLAYER,
        WAITING_FOR_PLAYER_CONFIG,
        WAITING_FOR_HOST_CONFIG,
        PLAYING,
        END_GAME
    }
}
