using AquaMan.Domain;
using AquaMan.Domain.Entity;
using AquaMan.Domain.GameEvent;
using AquaMan.DomainApi;
using AquaMan.WebsocketAdapter.Entity;
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
        

        private ConcurrentDictionary<Guid, ConnectedClient> connectedClients = new ConcurrentDictionary<Guid, ConnectedClient>();

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
                if(connectedClients.Count >= 4)
                {
                    throw new RoomIsFullException($@"Room is full, id: {ID}");
                }

                connectedClient = new ConnectedClient(
                        socket: socket,
                        account: account,
                        player: player,
                        slot: connectedClients.Count
                        );

                if (!connectedClients.TryAdd(
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

                if (!connectedClients.TryRemove(socket.ConnectionInfo.Id, out connectedClient))
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

        public void Shoot(IWebSocketConnection socket, string message)
        {
            //TODO: add throttle to limit the shoot range.

            var command = JsonConvert.DeserializeObject<Command<Shoot>>(message);

            ConnectedClient connectedClient;
            if (!connectedClients.TryGetValue(socket.ConnectionInfo.Id, out connectedClient))
            {
                throw new ConnectionNotFoundException(socket.ConnectionInfo.Id);
            }

            // TODO: create bullet order.

            connectedClient.Account.OnShootEvent(
                new ShootEvent(
                    shootBy: new ShootBy(
                        account: connectedClient.Account,
                        player: connectedClient.Player
                        ),
                    bulletName: "0",
                    cost: new Cost(
                        currency: connectedClient.Account.Wallet.Currency,
                        amount: 1,
                        precise: connectedClient.Account.Wallet.Precise
                        )
                    )
                );
            
            _accountService.Save(connectedClient.Account);

            var shotEvent = new Event<EventPayload.Shot>()
            {
                EventType = (int)EventType.Shot,
                Payload =
                {
                    Shooter = connectedClient.Account.Name,
                    Slot = connectedClient.Slot,
                    ShotBullet = new ShotBullet()
                    {
                        StartFrom = command.Payload.ShotBullet.StartFrom,
                        Direction = command.Payload.ShotBullet.Direction,
                        Bullet = command.Payload.ShotBullet.Bullet
                    }
                }
            };

            Broadcast(JsonConvert.SerializeObject(shotEvent));
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
            foreach (var connectedClient in connectedClients.Values)
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
