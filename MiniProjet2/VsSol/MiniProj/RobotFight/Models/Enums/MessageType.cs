using System;
using System.Collections.Generic;
using System.Text;

namespace RobotFight.Models.Enums
{
    public enum MessageType
    {
        PLAYER_JOIN,
        WELCOME,
        SERVER_BUSY,
        ROBOT_CONFIG,
        ROBOT_CONFIG_OK,
        ERROR,
        GAME_START,
        TURN,
        PLAYER_ACTION,
        PLAYER_RESULT,
        GAME_END,
        PLAYER_REPLAY,
        QUIT
    }
}
