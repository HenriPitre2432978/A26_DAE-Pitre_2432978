using RobotFight.Controllers;
using RobotFight.Models;
using RobotFight.Views;

namespace RobotFight
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                await new GameController(new ConsoleView()).Run();
            }
            catch (Exception ex)
            {
                //IF CRASH : self handler afficher par le programme
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine($"Erreur inattendue : \n\n{ex.Message}\n");
                Console.ResetColor();
                Console.WriteLine("Appuyez sur une touche pour fermer la fenêtre...");
                Console.ReadKey();
            }
        }
    }
}
