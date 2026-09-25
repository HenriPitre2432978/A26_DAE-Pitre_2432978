using RobotFight.Commands;
using RobotFight.Commands.Handlers;
using RobotFight.Connexion;
using RobotFight.MessageHandling;
using RobotFight.Models;
using RobotFight.Models.Enums;
using RobotFight.Views;
using System.Net;

namespace RobotFight.Controllers
{
    /// <summary>
    /// Appelé par Program, s'occupe de lier chaque composante principale du
    /// jeu, les connexion sockets et l'affichage.
    /// </summary>
    public class GameController(IGameView view)
    {
        #region Bases Communes aux deux côtés
        private readonly IGameView view = view;
        private readonly CommandMenu menu = new();
        private Game? game;
        private readonly object gameStartLock = new();   // protège TryStartGame() contre les deux threads qui pourraient le déclencher

        #region Propriétés Partagées

        private bool isHost;
        private GameStatus status = GameStatus.WAITING_FOR_PLAYER;

        #endregion

        #endregion

        #region Méthodes Communes

        /// <summary>
        /// Run initial: Demande si instance = S/C, Et poursuit le programme
        /// selon.
        /// </summary>
        public Task Run() =>
            view.AskPlayerType() == "S"
              ? CreateGame()
              : JoinGame(view.AskIpAddress(), view.AskPort());

        /// <summary>
        /// Diagram signature: l'état courant de la partie.
        /// </summary>
        public GameStatus GetGameStatus() => status;

        /// <summary>
        /// Assemble un message uniformisé qui peut être compris par n'importe
        /// quel programme compatible, (en tout cas, supposé...🙃)
        /// </summary>
        /// <param name="type">Type de message envoyé</param>
        /// <param name="args">Données utiles au réceptionnaire du msg</param>
        private Task Send(MessageType type, params object[] args)
        {
            IMessageSender sender = isHost ? server! : client!;
            return sender.Send(MessageHelper.Build(type, args));
        }

        /// <summary>
        /// Appelé à la suite d'une config terminée. Si valide, initialise la
        /// partie. 
        /// </summary>
        private async Task TryStartGame()
        {
            lock (gameStartLock)
            {
                if (hostRobotConfig == null || playerRobotConfig == null || game != null) return;

                Robot hostRobot = new(true, hostRobotConfig.ToStats());
                Robot clientRobot = new(false, playerRobotConfig.ToStats());
                game = new Game(hostRobot, clientRobot);
                game.StartGame();
                status = GameStatus.PLAYING;
            }

            view.ShowMessage("La partie commence !");

            //Envoyer début partie
            await Send(MessageType.GAME_START,
                game!.HostRobot.Hp, game.ClientRobot.Hp, game.HostRobot.Energy, game.ClientRobot.Energy);

            //L'hôte commence tjrs
            await PlayHostTurn();
        }

        /// <summary>
        /// Afficher action jouée localement + envoyer msg à serveur pour
        /// afficher à client
        /// </summary>
        /// <param name="actor">Qui a fait l'Action</param>
        /// <param name="action">Action jouée</param>
        /// <returns></returns>
        private Task TellResult(string actor, GameAction action)
        {
            //Si erreur (déco, etc) skip
            if (game == null) return Task.CompletedTask;

            view.ShowMessage($"{actor} joue {action} ({game.LastDamage} dégâts) — Hôte {game.HostRobot.Hp} PV, Client {game.ClientRobot.Hp} PV");

            //Envoyer résultat à serveur 
            return Send(MessageType.PLAYER_RESULT, actor, action, game.LastDamage,
                game.HostRobot.Hp, game.ClientRobot.Hp, game.HostRobot.Energy, game.ClientRobot.Energy);
        }

        /// <summary>
        /// Lancer fin partie (called lorsque robotpv = 0 ) (Calculer gagnant,
        /// Afficher resultats, send game end)
        /// </summary>
        private async Task EndGame()
        {
            //Skip si erreur 
            if (game == null) return;

            status = GameStatus.END_GAME;

            Robot? winner = game.GetWinner();
            view.ShowWinner(winner);

            //Convert winner to name of winner
            string winnerTxt;
            if (winner == null) winnerTxt = "AUCUN";
            else if (winner.IsHost)
                winnerTxt = "HOTE";
            else
                winnerTxt = "CLIENT";

            //Send winner to client to show winner on their eside
            await Send(MessageType.GAME_END, winnerTxt, game.HostRobot.Hp, game.ClientRobot.Hp);
        }

        #endregion

        #region Serveur
        private SocketServer? server;
        private RobotConfig? hostRobotConfig;

        public async Task CreateGame()
        {
            isHost = true;
            server = new SocketServer();

            //Associer le handler au message qui l'annonce
            menu.AddHandler(new PlayerJoinHandler(server, view, onHostJoined: PromptHostConfig), MessageType.PLAYER_JOIN);
            menu.AddHandler(new RobotReadyHandler(server, view, ConfigurePlayer), MessageType.ROBOT_CONFIG);
            menu.AddHandler(new PlayerActionHandler(server, view, OnClientAction), MessageType.PLAYER_ACTION);
            menu.AddHandler(new PlayerReplayHandler(server, view, onReplay: OnHostReplay), MessageType.PLAYER_REPLAY);
            menu.AddHandler(new QuitHandler(server, view, () => status = GameStatus.WAITING_FOR_PLAYER), MessageType.QUIT);

            //Lorsque server reçoit msg, menu trouve le
            //handler associé au msg reçu, le call et répond son résultat
            server.MessageReceived += menu.Execute;

            //EVENT lorsque déconnecté, reset game et wait for player
            server.PlayerDisconnected += OnPlayerDisconnected;

            //If everything OK, start server
            view.ShowMessage($"Serveur démarré sur le port {Config.PORT} à {Config.IP_ADDRESS}.");
            view.ShowMessage($"En attente d'un joueur...");


            //Start server jusque StopServer() called
            await server.StartServer(IPAddress.Parse(Config.IP_ADDRESS), Config.PORT);
        }

        #region Méthodes

        #region Related to RobotConfig

        /// <summary>
        /// (HANDLER)Server starts config and sets status to waiting for config
        /// </summary>
        private void PromptHostConfig()
        {
            status = GameStatus.WAITING_FOR_PLAYER_CONFIG;
            StartHostConfigThread();
        }

        /// <summary>
        /// Lit la config de l'hôte sur un thread séparé pour pas geler le
        /// program
        /// </summary>
        private void StartHostConfigThread()
        {
            Thread hostConfigThread = new(() =>
            {
                RobotConfig robotConfig = view.AskPlayerConfig(Config.POINTS_TO_GIVE);
                OnHostConfigured(robotConfig);
            });
            hostConfigThread.Start();
        }

        /// <summary>
        /// Enregistre et verrouille la config depuis callback, puis tente de
        /// démarrer la partie
        /// </summary>
        private void OnHostConfigured(RobotConfig robotConfig)
        {
            //Enregistrer config
            ConfigureHost(robotConfig);
            view.ShowMessage("Configuration verrouillée. En attente de l'adversaire...");

            //Démarre partie si client attendait après lui
            _ = TryStartGame();
        }
        /// <summary>
        /// Enregistrer la configuration robot de l'Host //TODO: UNIFORMISER
        /// CLIENT ET HSOT CONFIGROBOT
        /// </summary>
        public void ConfigureHost(RobotConfig robotConfig) => hostRobotConfig = robotConfig;

        #endregion

        private void OnPlayerDisconnected()
        {
            //Nullify all states to default
            status = GameStatus.WAITING_FOR_PLAYER;
            hostRobotConfig = null;
            playerRobotConfig = null;
            game = null;

            //Annoncer reconnexion
            view.ShowMessage($"Serveur démarré sur le port {Config.PORT} à {Config.IP_ADDRESS}.");
            view.ShowMessage("Le joueur s'est déconnecté. En attente d'un joueur...");
        }

        /// <summary>
        /// Tour de l'hôte demandé en local, rejoué tant que refusé.
        /// </summary>
        private async Task PlayHostTurn()
        {
            if (game == null) return;

            //Tell client host is playing ("Tour de l'hôte...")
            await Send(MessageType.TURN, "HOTE");

            bool played;
            do
            {
                //Choose action from choices
                GameAction action = view.AskPlayerAction();

                //TryPlay
                played = game.Play(true, action);

                //Catch (si pas assez d'énergie)
                if (!played) view.ShowMessage("Énergie insuffisante : choisissez une autre action.");

                //afficher resultat
                else await TellResult("HOTE", action);
            } while (!played);

            //Check if robotpv=0
            if (game.CheckGameEnded()) { await EndGame(); return; }

            //Switch turn to client
            await Send(MessageType.TURN, "CLIENT");
        }

        /// <summary>
        /// CALLED par handler: lorsque client souhaite replay, reset all and
        /// restart host config
        /// </summary>
        private void OnHostReplay()
        {
            hostRobotConfig = null;
            playerRobotConfig = null;
            game = null;
            PromptHostConfig();
        }

        #endregion

        #endregion

        #region Client
        private SocketClient? client;
        private RobotConfig? playerRobotConfig;

        #region Méthodes
        /// <summary>
        /// CLIENT
        /// Associe les handlers aux msg a envoyer au serveur et à leur méthode principale,
        /// EVENT Reçoit et éxécute les messages reçus
        /// EVENT Gère l'action de déconnexion
        /// Connecte le client au serveur
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public async Task JoinGame(string ipAddress, int port)
        {
            isHost = false;
            client = new SocketClient();
            var done = new TaskCompletionSource();

            menu.AddHandler(new PlayerJoinHandler(client, view, onWelcome: SendRobotConfig), MessageType.WELCOME);
            menu.AddHandler(new RobotReadyHandler(client, view, onActionRefused: RetryClientAction), MessageType.ROBOT_CONFIG_OK, MessageType.ERROR);
            menu.AddHandler(new PlayerActionHandler(client, view), MessageType.TURN);
            menu.AddHandler(new PlayerResultHandler(client, view), MessageType.GAME_START, MessageType.PLAYER_RESULT, MessageType.GAME_END);
            menu.AddHandler(new PlayerReplayHandler(client, view, onReplayOk: SendRobotConfig), MessageType.PLAYER_REPLAY);
            menu.AddHandler(new PlayerLeaveHandler(client, view, client.Exit), MessageType.SERVER_BUSY);
            menu.AddHandler(new QuitHandler(client, view, client.Exit), MessageType.QUIT);

            client.MessageReceived += menu.Execute;

            //Forcer Done à complete la task pour déconnecter le joueur
            client.Disconnected += () => done.TrySetResult();

            //Connecter le client
            await client.ConnectToServer(ipAddress, port);

            //Envoyer msg de join à serveur. serv gère l'information de son côté
            await client.Send(MessageHelper.Build(MessageType.PLAYER_JOIN));

            //Attendre que task soit done
            await done.Task;
            view.ShowMessage("Déconnecté.");
        }

        /// <summary>
        /// Demande une config au CLIENT et l'envoie.  Utilisé au handler
        /// WELCOME et après chaque REPLAY
        /// </summary>
        private async Task SendRobotConfig()
        {
            //Get config from client
            RobotConfig robot = view.AskPlayerConfig(Config.POINTS_TO_GIVE);

            //Send config to server to validate and continue
            await client!.Send(MessageHelper.Build(MessageType.ROBOT_CONFIG, robot.HpPoints, robot.ArmorPoints, robot.DamagePoints));
        }

        /// <summary>
        /// redemander action si fail action client
        /// </summary>
        private async Task RetryClientAction() =>
            await client!.Send(MessageHelper.Build(MessageType.PLAYER_ACTION, view.AskPlayerAction()));

        /// <summary>
        /// Enregistre la config du client (Non optimisé(répétition); TODO:
        /// uniformiser ConfigurePlayer et ConfigureHost )
        /// </summary>
        /// <param name="robotConfig"></param>
        /// <returns></returns>
        public async Task ConfigurePlayer(RobotConfig robotConfig)
        {
            playerRobotConfig = robotConfig;
            await TryStartGame();
        }

        /// <summary>
        /// HANDLER appelé lorsuq ele client doit jouer son tour
        /// </summary>
        private async Task OnClientAction(GameAction action)
        {
            //skip si erreur de restart
            if (game == null) return;

            if (!game.Play(false, action))
            {
                await Send(MessageType.ERROR, "ACTION");

                // redemander action si pas réussi
                return;
            }

            await TellResult("CLIENT", action);

            //Check si robotpv= 0, si oui terminer partie
            if (game.CheckGameEnded()) { await EndGame(); return; }

            //Switch turn
            await PlayHostTurn();
        }

        #endregion


        #endregion

    }
}
