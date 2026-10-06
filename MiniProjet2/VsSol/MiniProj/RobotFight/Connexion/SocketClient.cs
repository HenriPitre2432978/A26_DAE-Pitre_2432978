using RobotFight.Models;
using System.Net;
using System.Net.Sockets;
using Base = BibliothequeFonctionsDeBase.FonctionsDeBase;

namespace RobotFight.Connexion
{

    public class SocketClient : IMessageSender
    {
        private ConnectionHandler? connection;

        public event Func<Message, Task>? MessageReceived;
        public event Action? Disconnected;
        public bool IsConnected() => connection?.IsConnected ?? false;

        private static readonly TimeSpan ConnectTimeoutS = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Tente de se connecter au serveur. Ne lance JAMAIS d'exception réseau :
        /// en cas d'échec, retourne un message d'erreur lisible pour l'utilisateur.
        /// </summary>
        /// <param name="ipAddress">IPv4 complète du serveur (ex: 192.168.1.10)</param>
        /// <param name="port">Port du serveur</param>
        /// <returns>null si connecté, sinon le message d'erreur à afficher</returns>
        public async Task<string?> ConnectToServer(string ipAddress, int port)
        {
            if (!Base.EstIPValide(ipAddress))
                return "Adresse IP invalide. Entrez les 4 nombres, ex: 192.168.1.10";

            if (port is < 1 or > 65535)
                return "Port invalide (1 à 65535).";

            IPAddress ip = IPAddress.Parse(ipAddress);

            //0.0.0.0 == goto localhost
            if (ip.Equals(IPAddress.Any))
                ip = IPAddress.Loopback;

            Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //TRY CONNEXION
            try
            {
                //self timeout pour éviter 20 sec attente erreur
                using CancellationTokenSource cts = new(ConnectTimeoutS);
                await socket.ConnectAsync(new IPEndPoint(ip, port), cts.Token);
            }

            //CATCH ABORT
            catch (OperationCanceledException)
            {
                socket.Dispose();
                return $"Aucune réponse de {ip}/{port} en {ConnectTimeoutS.TotalSeconds:0} s. " +
                       "Vérifiez l'IP, le statut du serveur, l'autorisation du port dans le pare-feu, et le réseau.";
            }

            //CATCH SOCKET ERROR
            catch (SocketException ex)
            {
                socket.Dispose();
                return ex.SocketErrorCode switch
                {
                    SocketError.ConnectionRefused => $"Connexion refusée par {ip}:{port}. Démarrez le serveur !",
                    SocketError.HostUnreachable or SocketError.NetworkUnreachable => $"{ip} n'a pu être atteint !",
                    SocketError.TimedOut => $"Aucune réponse de {ip}:{port} (Time Out !)",
                    _ => $"Connexion impossible : {ex.Message}."
                };
            }

            //CATCH OTRHER
            catch (Exception ex)
            {
                socket.Dispose();
                return $"Connexion impossible : {ex.Message}";
            }

            connection = new ConnectionHandler(socket);
            _ = Listen(connection);
            return null;
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
