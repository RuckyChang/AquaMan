using AquaMan.Domain;
using AquaMan.DomainApi;
using AquaMan.WebsocketAdapter.Exceptions;
using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;

namespace AquaMan.WebsocketAdapter
{
    public class GameRoom
    {
        private object _joinRoomLock = new object();
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
            var command = JsonConvert.DeserializeObject<Command<CommandPayload.JoinGame>>(message);
            
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

            int slot = 0;

            lock (_joinRoomLock)
            {
                if(sockets.Count >= 4)
                {
                    throw new RoomIsFullException($@"Room is full, id: {ID}");
                }

                slot = sockets.Count;

                if (!sockets.TryAdd(
                    socket.ConnectionInfo.Id,
                    new ConnectedClient(
                        socket: socket,
                        account: account,
                        player: player
                        )
                    )){
                    throw new JoinGameFailedException($@"Join game failed: {socket.ConnectionInfo.Id}, Id: {ID}");
                }
            }

            Broadcasd(EventType.JoinedGame, new EventPayload.JoinedGame()
            {
                Name = account.Name,
                Slot = slot
            });
        }






        private void Broadcasd<TPayload>(EventType eventType, TPayload payload)
        {
            foreach(var connectedClient in sockets.Values)
            {
                var event2Send = new Event<TPayload>()
                {
                    EventType = (int)eventType,
                    Payload = payload
                };

                connectedClient.Socket.Send(JsonConvert.SerializeObject(event2Send));
            }
        }

        public class ConnectedClient
        {
            public IWebSocketConnection Socket { get;}
            public Player Player { get; }

            public Account Account { get; }

            public ConnectedClient(IWebSocketConnection socket, Player player, Account account)
            {
                Socket = socket;
                Player = player;
                Account = account;
            }
        }
    }
}
