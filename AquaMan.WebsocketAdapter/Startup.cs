using AquaMan.DomainApi;
using AquaMan.WebsocketAdapter.Exceptions;
using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace AquaMan.WebsocketAdapter
{
    public class Startup
    {
        WebSocketServer _server;
        private Lobby _lobby;
        // only one game room right now.
        private GameRoom _gameRoom;
        private ConcurrentDictionary<Guid, IWebSocketConnection> sockets = new ConcurrentDictionary<Guid, IWebSocketConnection>();

        public Startup(
            int port,
            BulletOrderService bulletOrderService,
            AccountService accountService,
            PlayerService playerService
            )
        {
            _server = new WebSocketServer("ws://0.0.0.0:" + port);
            _lobby = new Lobby(accountService);
            _gameRoom = new GameRoom(
                gameId: Guid.NewGuid().ToString(),
                id: Guid.NewGuid().ToString(), 
                bulletOrderService: bulletOrderService,
                accountService: accountService, 
                playerService: playerService
            );
        }

        public void Start()
        {
            _server.Start(socket => {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    if (!sockets.TryAdd(socket.ConnectionInfo.Id, socket))
                    {
                        Console.WriteLine(string.Format("register Socket failed, {0}", socket.ConnectionInfo.Id));
                        socket.Send("register Socket failed");
                        socket.Close();
                    }
                };
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                    // parse message
                    HandleMessge(socket, message);
                };
            });
        }

        
        public void HandleMessge(IWebSocketConnection socket, string message)
        {
            var command = JsonConvert.DeserializeObject<Command>(message);
            try
            {
                switch ((CommandType)command.CommandType)
                {
                    case CommandType.Login:
                        _lobby.Login(socket, message);
                        break;
                    case CommandType.Logout:
                        _lobby.Logout(socket, message);
                        break;
                    case CommandType.JoinGame:
                        _gameRoom.JoinGame(socket, message);
                        break;
                    case CommandType.QuitGame:
                        _gameRoom.QuitGame(socket, message);
                        break;
                    case CommandType.Shoot:
                        _gameRoom.Shoot(socket, message);
                        break;
                    case CommandType.HitTarget:
                        _gameRoom.Hit(socket, message);
                        break;
                    default:
                        throw new NoSuchCommandException(command.CommandType);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                socket.Send(JsonConvert.SerializeObject(
                    new Event<EventPayload.Error>()
                    {
                        EventType = -1,
                        Payload = new EventPayload.Error()
                        {
                            ErrorCode = "-1",
                            ErrorMessage = exception.Message
                        }
                    }
                    ));
            }
        }
    }
}
