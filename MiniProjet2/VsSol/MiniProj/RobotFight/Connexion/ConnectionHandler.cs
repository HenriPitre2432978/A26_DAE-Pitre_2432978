using RobotFight.MessageHandling;
using RobotFight.Models;
using RobotFight.Models.Enums;
using System.Net.Sockets;
using System.Text;

namespace RobotFight.Connexion
{
    public interface IMessageSender
    {
        Task Send(Message message);
    }

    /// <summary>
    /// Gestion de la connexion TCP.
    /// Gère les requêtes ligne par ligne afin d'éviter que deux requêtes
    /// s'entrebouffent.
    /// </summary>
    public sealed class ConnectionHandler : IDisposable
    {
        private readonly Socket socket;

        /// <summary>
        /// Lock pour éviter d'envoyer deux messages en même temps.
        /// </summary>
        private readonly SemaphoreSlim sendLock = new(1, 1);

        /// <summary>
        /// Buffer utilisé pour recevoir les données TCP.
        /// </summary>
        private readonly byte[] receiveBuffer = new byte[4096];

        private string receiveData = string.Empty;

        private const string EOM = "<|EOM|>";

        //nb erreurs recues
        private int consecutiveFormatErrors;

        public bool IsConnected => socket.Connected;

        public ConnectionHandler(Socket socket)
        {
            this.socket = socket;
        }

        /// <summary>
        /// Envoie un message sur le socket.
        /// Chaque message est terminé par un '\n'.
        /// </summary>
        public async Task SendMessage(Message message)
        {
            await sendLock.WaitAsync();

            try
            {
                string serialized = MessageHelper.Serialize(message) + EOM;
                byte[] bytes = Encoding.UTF8.GetBytes(serialized);

                await socket.SendAsync(
                    bytes,
                    SocketFlags.None
                );
            }
            finally
            {
                sendLock.Release();
            }
        }

        /// <summary>
        /// Reçoit le prochain message complet.
        /// Les messages sont séparés par '\n'.
        /// </summary>
        public async Task<Message?> ReceiveMessage()
        {
            while (true)
            {
                int newlineIndex = receiveData.IndexOf(EOM);

                if (newlineIndex >= 0)
                {
                    string line = receiveData[..newlineIndex];
                    receiveData = receiveData[(newlineIndex + EOM.Length)..];

                    line = line.TrimEnd('\r');

                    if (MessageHelper.TryParseMessage(
                        line,
                        out Message? message))
                    {
                        consecutiveFormatErrors = 0;
                        return message;
                    }

                    if (++consecutiveFormatErrors <= 2)
                        await SendMessage(
                            MessageHelper.Build(MessageType.ERROR, "FORMAT DE REQUETE")
                        );

                    continue;
                }

                int bytesReceived;

                try
                {
                    bytesReceived = await socket.ReceiveAsync(
                        receiveBuffer,
                        SocketFlags.None
                    );
                }
                catch (SocketException)
                {
                    return null;
                }

                // 0 bytes = connexion fermée
                if (bytesReceived == 0)
                    return null;

                string chunk = Encoding.UTF8.GetString(
                    receiveBuffer,
                    0,
                    bytesReceived
                );
                receiveData += chunk;
            }
        }

        /// <summary>
        /// Écoute continuellement les messages reçus.
        /// </summary>
        public async Task Listen(Func<Message, Task> onMessage)
        {
            try
            {
                Message? message;

                while ((message = await ReceiveMessage()) != null)
                {
                    await onMessage(message);
                }
            }
            catch (SocketException)
            {
                // Connexion perdue
            }
            catch (ObjectDisposedException)
            {
                // Socket fermé volontairement
            }
        }

        public void Dispose()
        {
            socket.Dispose();
            sendLock.Dispose();
        }
    }
}
