using AquaMan.Domain;
using AquaMan.DomainApi;
using AquaMan.WebsocketAdapter.Exceptions;
using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using static AquaMan.WebsocketAdapter.CommandPayload;

namespace AquaMan.WebsocketAdapter
{
    public class GameRoom
    {
        private object _roomLock = new object();
        
        public string ID { get; }
        private AccountService _accountService;
        private PlayerService _playerService;

        private ConcurrentDictionary<Guid, ConnectedClient> sockets = new ConcurrentDictionary<Guid, ConnectedClient>();

        public GameRoom(
            string id,
            AccountService accountService,
            PlayerService playerService
            )
        {
            ID = id;
            _accountService = accountService;
            _playerService = playerService;
        }

        public void JoinGame(IWebSocketConnection socket, string message)
        {
            var command = JsonConvert.DeserializeObject<Command<JoinGame>>(message);
            
            var account = _accountService.OfToken(command.Payload.Token);

            if(account == null)
            {
                throw new NoSuchTokenException(command.Payload.Token);
            }

            // create player
            var player = _playerService.OfAccountId(
                accountId: account.ID
                );

            player.OnJoinGame(ID);
            _playerService.Save(player);
            ConnectedClient connectedClient;
            lock (_roomLock)
            {
                if(sockets.Count >= 4)
                {
                    throw new RoomIsFullException($@"Room is full, id: {ID}");
                }

                connectedClient = new ConnectedClient(
                        socket: socket,
                        account: account,
                        player: player,
                        slot: sockets.Count
                        );

                if (!sockets.TryAdd(
                        socket.ConnectionInfo.Id,
                        connectedClient
                    )
                ){
                    throw new JoinGameFailedException($@"Join game failed: {socket.ConnectionInfo.Id}, Id: {ID}");
                }
            }

            Broadcast(EventType.JoinedGame, new EventPayload.JoinedGame()
            {
                Name = account.Name,
                Slot = connectedClient.Slot
            });
        }

        public void QuitGame(IWebSocketConnection socket, string message)
        {
            var command = JsonConvert.DeserializeObject<Command<QuitGame>>(message);

            ConnectedClient connectedClient;
            lock (_roomLock)
            {

                if (!sockets.TryRemove(socket.ConnectionInfo.Id, out connectedClient))
                {
                    throw new JoinGameFailedException($@"Join game failed: {socket.ConnectionInfo.Id}, Id: {ID}");
                }
            }

            var player = _playerService.OfAccountId(connectedClient.Account.ID);

            player.OnQuitGame();

            var event2Send = new Event<EventPayload.QuitGame>()
            {
                EventType = (int)EventType.QuitGame,
                Payload = new EventPayload.QuitGame()
                {
                    Name = connectedClient.Account.Name,
                    Slot = connectedClient.Slot
                }
            };

            var message2Send = JsonConvert.SerializeObject(event2Send);

            connectedClient.Socket.Send(message2Send);

            Broadcast(message2Send);
        }

     

        private void Broadcast<TPayload>(EventType eventType, TPayload payload)
        {
            var event2Send = new Event<TPayload>()
            {
                EventType = (int)eventType,
                Payload = payload
            };
            Broadcast(JsonConvert.SerializeObject(event2Send));
        }

        private void Broadcast(string message)
        {
            foreach (var connectedClient in sockets.Values)
            {
                connectedClient.Socket.Send(message);
            }
        }

        public class ConnectedClient
        {
            public IWebSocketConnection Socket { get;}
            public Player Player { get; }

            public Account Account { get; }
            public int Slot { get;  }

            public ConnectedClient(
                IWebSocketConnection socket, 
                Player player, 
                Account account,
                int slot
                )
            {
                Socket = socket;
                Player = player;
                Account = account;
                Slot = slot;
            }
        }
    }
}
