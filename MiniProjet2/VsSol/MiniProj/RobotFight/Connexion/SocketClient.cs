using RobotFight.Models;
using System.Net.Sockets;
using System.Numerics;

namespace RobotFight.Connexion
{

    public class SocketClient : IMessageSender
    {
        private ConnectionHandler? connection;

        public event Func<Message, Task>? MessageReceived;
        public event Action? Disconnected;
        public bool IsConnected() => connection?.IsConnected ?? false;

        public async Task ConnectToServer(string ipAddress, int port)
        {
            Socket socket = new(
        AddressFamily.InterNetwork,
        SocketType.Stream,
        ProtocolType.Tcp
    );

            await socket.ConnectAsync(ipAddress, port);

            connection = new ConnectionHandler(socket);

            _ = Listen(connection);
        }


        public Task Send(Message message) =>
            connection?.SendMessage(message) ?? throw new InvalidOperationException("Pas connecté au serveur");

        public void Exit()
        {
            connection?.Dispose();
            connection = null;
        }

        private async Task Listen(ConnectionHandler connection)
        {
            try
            {
                //Listen pour les msg du serv et invoke le callback pour dispatch
                await connection.Listen(m => MessageReceived?.Invoke(m) ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Erreur : {ex.Message}");
            }
            finally
            {
                //Dispatch lorsque l'event disconnected est triggered
                Disconnected?.Invoke();
            }
        }
    }
}
