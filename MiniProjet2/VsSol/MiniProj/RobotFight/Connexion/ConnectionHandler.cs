using RobotFight.MessageHandling;
using RobotFight.Models;
using RobotFight.Models.Enums;
using System.Net.Sockets;
using System.Text;

namespace RobotFight.Connexion
{
    public interface IMessageSender { Task Send(Message message); }

    /// <summary>
    /// Gestion de la connexion  TCP:
    /// Gère les requêtes en regardant ligne par ligne, pour éviter que deux requêtes s'entrebouffent
    /// </summary>
    public sealed class ConnectionHandler : IDisposable
    {
        private readonly TcpClient socket;
        private readonly StreamReader input;
        private readonly StreamWriter output;

        /// <summary>
        /// Lock le thread pour éviter deux messages en même temps
        /// </summary>
        private readonly SemaphoreSlim sendLock = new(1, 1);

        public bool IsConnected => socket.Connected;

        public ConnectionHandler(TcpClient socket)
        {
            this.socket = socket;
            NetworkStream stream = socket.GetStream();
            input = new StreamReader(stream, Encoding.UTF8);
            output = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true, NewLine = "\n" };
        }

        /// <summary>
        /// Lock le thread, convertit le message et le send à l'output, unlock le thread
        /// </summary>
        /// <param name="message">Message à envoyer</param>
        /// <returns></returns>
        public async Task SendMessage(Message message)
        {
            await sendLock.WaitAsync();
            try { await output.WriteLineAsync(MessageHelper.Serialize(message)); }
            finally { sendLock.Release(); }
        }


        /// <summary>
        /// Toujours open, reçoit null si rien reçu. Parse le message lorsque reçu et le retourne
        /// </summary>
        /// <returns>Le message parsed</returns>
        public async Task<Message?> ReceiveMessage()
        {
            while (true)
            {
                string? line = await input.ReadLineAsync();
                if (line == null) return null;

                if (MessageHelper.TryParseMessage(line, out Message? message))
                    return message;

                //Send error system format message
                await SendMessage(MessageHelper.Build(MessageType.ERROR, "FORMAT"));
            }
        }

        /// <summary>
        /// Pour chaque message reçu, callback onMessage lorsque ReceiveMessage ne reçoit pas rien
        /// </summary>
        /// <param name="onMessage">EVENT callback qui fait le rappel (trigger?)</param>
        public async Task Listen(Func<Message, Task> onMessage)
        {
            try
            {
                Message? message;
                while ((message = await ReceiveMessage()) != null)
                    await onMessage(message);
            }
            catch (IOException) { }             // connection dropped
            catch (ObjectDisposedException) { } // we closed it ourselves
        }

        /// <summary>
        /// Jeter le message lorsque terminé pour pas le laisser dans le thread sans raison
        /// </summary>
        public void Dispose()
        {
            input.Dispose();
            output.Dispose();
            socket.Dispose();
            sendLock.Dispose();
        }
    }
}
