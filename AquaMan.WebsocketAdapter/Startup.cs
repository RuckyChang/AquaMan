using AquaMan.DomainApi;
using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using static AquaMan.WebsocketAdapter.Lobby;

namespace AquaMan.WebsocketAdapter
{
    public class Startup
    {
        WebSocketServer _server;
        ConcurrentDictionary<Guid, IWebSocketConnection> sockets = new ConcurrentDictionary<Guid, IWebSocketConnection>();

        private Lobby _lobby;

        public Startup(AccountService accountService)
        {

            _lobby = new Lobby(accountService);

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
                    case CommandType.ListGame:
                        _lobby.ListGame(socket, message);
                        break;
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
                            ErrorCode = "-1"
                        }
                    }
                    ));
            }
        }
    }
}
