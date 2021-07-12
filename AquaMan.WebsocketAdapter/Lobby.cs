using System;
using System.Collections.Concurrent;
using Fleck;
using Newtonsoft.Json;

namespace AquaMan.WebsocketAdapter
{
    public partial class Lobby
    {
        WebSocketServer _server;
        ConcurrentDictionary<Guid, IWebSocketConnection> sockets = new ConcurrentDictionary<Guid, IWebSocketConnection>();
        public Lobby(int port=8081)
        {
            _server = new WebSocketServer("ws://0.0.0.0:"+port);
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
                    ParseMessage(message);
                };
            });
        }

        public void ParseMessage(string message)
        {
            var command = JsonConvert.DeserializeObject<Command>(message);
            switch ((CommandType)command.CommandType)
            {
                case CommandType.Login:
                    Command<Login> loginCommand = JsonConvert.DeserializeObject<Command<Login>>(message);
                    Console.WriteLine(loginCommand.Payload.Name);
                    Console.WriteLine(loginCommand.Payload.Password);
                    break;
                case CommandType.Logout:

                    break;
            }
        }

        public void CommandDispatcher()
        {

        }
    }
}
