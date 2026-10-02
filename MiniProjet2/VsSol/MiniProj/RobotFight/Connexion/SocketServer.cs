using RobotFight.MessageHandling;
using RobotFight.Models;
using RobotFight.Models.Enums;
using RobotFight.Views;
using System.Net;
using System.Net.Sockets;

namespace RobotFight.Connexion
{
    /// <summary>Accepts one player. Anyone else who connects gets SERVER_BUSY.</summary>
    public class SocketServer : IMessageSender
    {

        private ConnectionHandler? player;

        public event Func<Message, Task>? MessageReceived;
        public event Action? PlayerDisconnected;

        private Socket? listener;

        public async Task StartServer(IPAddress ip, int port)
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(new IPEndPoint(ip, port));
                listener.Listen(1); // backlog : nb de connexions en attente

                while (true)
                {
                    Socket socket = await listener.AcceptAsync();
                    ConnectionHandler connection = new(socket);

                    if (player != null)
                    {
                        await connection.SendMessage(MessageHelper.Build(MessageType.SERVER_BUSY));
                        connection.Dispose();
                        continue;
                    }

                    //Connect la connexion potentielle au socket client
                    player = connection;

                    //TODO: DANGER : EVIL: DIS LE COMME TU VEUX
                    ConsoleView.BaseDisplay("HÔTE | PARTIE EN COURS ");

                    //Listen le client et dispatch le msg au Invoke (callback) 
                    _ = ServePlayer(connection);

                }
            }
            //Si socket terminé ou client est disposed
            catch (SocketException) { }
            catch (ObjectDisposedException) { }
        }

        public void StopServer()
        {
            player?.Dispose();
            player = null;
            listener?.Dispose(); //exception si loop infinie
            listener = null;
        }

        public Task Send(Message message) =>
            player?.SendMessage(message) ?? Task.CompletedTask;

        private async Task ServePlayer(ConnectionHandler connection)
        {
            try
            {
                //Listen pour les msg du client et invoke le callback pour dispatch
                await connection.Listen(m => MessageReceived?.Invoke(m) ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                //Prob connexion 
                Console.Error.WriteLine($"Erreur : {ex.Message}");
            }
            finally
            {
                //Terminer et jeter connexion
                connection.Dispose();
                player = null;
                PlayerDisconnected?.Invoke();
            }
        }
    }
}
