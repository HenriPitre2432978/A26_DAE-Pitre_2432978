using RobotFight.Controllers;
using RobotFight.Models;
using RobotFight.Views;

namespace RobotFight
{
    internal class Program
    {
        static Task Main(string[] args)
        {
            Task GC = new GameController(new ConsoleView()).Run();
            return GC;
        }
    }
}
