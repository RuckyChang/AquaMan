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
using System.Collections.Generic;
using System.Threading.Tasks;
using static AquaMan.WebsocketAdapter.CommandPayload;
using static AquaMan.WebsocketAdapter.EventPayload;

namespace AquaMan.WebsocketAdapter
{
    public class GameRoom
    {
        private object _roomLock = new object();
        
        public string GameID { get; }
        public string ID { get; }
        private AccountService _accountService;
        private PlayerService _playerService;
        private BulletOrderService _bulletOrderService;

        private bool[] Slots = { false, false, false, false };

        private ConcurrentDictionary<Guid, ConnectedClient> connectedClients = new ConcurrentDictionary<Guid, ConnectedClient>();
        private SmallEnemyFactory enemyFactory;


        public GameRoom(
            string gameId,
            string id,
            AccountService accountService,
            PlayerService playerService,
            BulletOrderService bulletOrderService
            )
        {
            GameID = gameId;
            ID = id;
            _accountService = accountService;
            _playerService = playerService;
            _bulletOrderService = bulletOrderService;
            enemyFactory = new SmallEnemyFactory();
            initRespawningEnemies();
        }

        private void initRespawningEnemies()
        {
            var _ = Task.Run(() =>
            {
                var random = new Random();
                var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                while (true)
                {
                    var current = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (timestamp + 1000 < current)
                    {
                        var enemies = enemyFactory.RespawnEnemies(5);
                        timestamp = current;
                        var respawnEnemiesMessage = JsonConvert.SerializeObject(new Event<RespawnedEnemies>()
                        {
                            EventType = (int)EventType.RespawnedEnemies,
                            Payload = new RespawnedEnemies()
                            {
                                Enemies = enemies
                            }
                        });

                        Broadcast("", respawnEnemiesMessage);
                    }
                }
            });
        }

        public void JoinGame(IWebSocketConnection socket, string message)
        {
            var command = JsonConvert.DeserializeObject<Command<JoinGame>>(message);

            var account = _accountService.OfToken(command.Payload.Token);

            if (account == null)
            {
                throw new NoSuchTokenException(command.Payload.Token);
            }

        
            // create player
            var player = _playerService.OfAccountId(
                accountId: account.ID
                );

            
            _playerService.Save(player);
            ConnectedClient connectedClient;
            lock (_roomLock)
            {

                int slot = ReserveEmptySlot();
                if (slot < 0)
                {
                    throw new RoomIsFullException($@"Room is full, id: {ID}");
                }
                

                connectedClient = new ConnectedClient(
                        socket: socket,
                        account: account,
                        player: player,
                        slot: slot // not correct
                        );

                if (!connectedClients.TryAdd(
                        socket.ConnectionInfo.Id,
                        connectedClient
                    )
                ){
                    throw new JoinGameFailedException($@"Join game failed: {socket.ConnectionInfo.Id}, Id: {ID}");
                }

                player.OnJoinGame(ID);
            }

            var toPlayerItSelftmessage = JsonConvert.SerializeObject(new Event<EventPayload.JoinedGame>
            {
                EventType = (int)EventType.JoinedGame,
                Payload = new EventPayload.JoinedGame()
                {
                    ID = player.ID,
                    Name = account.Name,
                    Slot = connectedClient.Slot,
                    RoomId = command.Payload.RoomId
                }
            });

            connectedClient.Socket.Send(toPlayerItSelftmessage);

            var boradCastMessage = JsonConvert.SerializeObject(new Event<EventPayload.PlayerJoinedGame>()
            {
                EventType = (int)EventType.PlayerJoinedGame,
                Payload = new EventPayload.PlayerJoinedGame()
                {
                  PlayerInfo = new PlayerInfo()
                  {
                      ID = connectedClient.Player.ID,
                      Name = connectedClient.Account.Name,
                      Slot = connectedClient.Slot,
                      Money = new Money(
                        connectedClient.Account.Wallet.Currency,
                        connectedClient.Account.Wallet.Amount,
                        connectedClient.Account.Wallet.Precise
                        ),
                      Body = new Body()
                      {
                          Rotation = connectedClient.Rotation
                      }
                  }
                }
            });

            Broadcast(connectedClient.Socket.ConnectionInfo.Id, boradCastMessage);
        }

        private int ReserveEmptySlot()
        {
            for(int i =0; i< Slots.Length; i++)
            {
                if (!Slots[i])
                {
                    Slots[i] = true;
                    return i;
                }
            }

            return -1;
        }

        public void OnCloseConnection(IWebSocketConnection socket)
        {
            ConnectedClient connectedClient;
            lock (_roomLock)
            {

                if (!connectedClients.TryRemove(socket.ConnectionInfo.Id, out connectedClient))
                {
                    throw new QuitGameFailedException(@$"could not find the player with connection id: {socket.ConnectionInfo.Id}");
                }

                this.Slots[connectedClient.Slot] = false;
            }

            var player = _playerService.OfAccountId(connectedClient.Account.ID);

            player.OnQuitGame();

            var event2Send = new Event<EventPayload.PlayerQuitGame>()
            {
                EventType = (int)EventType.PlayerQuitGame,
                Payload = new EventPayload.PlayerQuitGame()
                {
                    ID = connectedClient.Player.ID,
                    Name = connectedClient.Account.Name,
                    Slot = connectedClient.Slot
                }
            };

            var message2Send = JsonConvert.SerializeObject(event2Send);

            Broadcast(connectedClient.Socket.ConnectionInfo.Id, message2Send);
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
                this.Slots[connectedClient.Slot] = false;
            }

            var player = _playerService.OfAccountId(connectedClient.Account.ID);

            player.OnQuitGame();

            var event2Send = new Event<EventPayload.PlayerQuitGame>()
            {
                EventType = (int)EventType.PlayerQuitGame,
                Payload = new EventPayload.PlayerQuitGame()
                {
                    ID = connectedClient.Player.ID,
                    Name = connectedClient.Account.Name,
                    Slot = connectedClient.Slot
                }
            };

            var message2Send = JsonConvert.SerializeObject(event2Send);

            connectedClient.Socket.Send(message2Send);

            Broadcast(connectedClient.Socket.ConnectionInfo.Id, message2Send);
        }

        public void Shoot(IWebSocketConnection socket, string message)
        {
           
            var command = JsonConvert.DeserializeObject<Command<Shoot>>(message);

            ConnectedClient connectedClient;
            if (!connectedClients.TryGetValue(socket.ConnectionInfo.Id, out connectedClient))
            {
                throw new ConnectionNotFoundException(socket.ConnectionInfo.Id);
            }

            connectedClient.Shoot();

            var account = connectedClient.Account;

            _bulletOrderService.Create(
                agentId: account.AgentId,
                gameId: GameID,
                gameRoomId: ID,
                accountId: account.ID,
                cost: GetBulletCost(command.Payload.ShotBullet.BulletID)
            );

            _accountService.Save(connectedClient.Account);

            var shotEvent = new Event<EventPayload.Shot>()
            {
                EventType = (int)EventType.Shot,
                Payload = new EventPayload.Shot()
                {
                    Shooter = connectedClient.Player.ID,
                    Slot = connectedClient.Slot,
                    ShotBullet = new ShotBullet()
                    {
                        StartFrom = command.Payload.ShotBullet.StartFrom,
                        Direction = command.Payload.ShotBullet.Direction,
                        BulletID = command.Payload.ShotBullet.BulletID
                    }
                }
            };

            Broadcast(connectedClient.Socket.ConnectionInfo.Id, JsonConvert.SerializeObject(shotEvent));
        }

        public void Hit(IWebSocketConnection socket, string message)
        {
            var command = JsonConvert.DeserializeObject<Command<CommandPayload.HitTarget>>(message);

            ConnectedClient connectedClient;
            if (!connectedClients.TryGetValue(socket.ConnectionInfo.Id, out connectedClient))
            {
                throw new ConnectionNotFoundException(socket.ConnectionInfo.Id);
            }

            var hitEnemy = enemyFactory.FindEnemy(command.Payload.InGameId);

            //TODO: this may cause race condition, try to add lock or wirte another actor to handle this.
            var bulletOrder = _bulletOrderService.ofAccountId(connectedClient.Account.ID, BulletOrderStateType.FIRED, 1)[0];
            bulletOrder.OnHit(new Domain.HitTarget(hitEnemy.ID));
            _bulletOrderService.Save(bulletOrder);

            //TODO: find it in the group
            var enemy = new Domain.Entity.Enemy(Guid.NewGuid().ToString());

            var cost = new Cost(currency: 0, amount: 100, precise: 100);
            var bullet = new Domain.Entity.Bullet("0", cost);

            (bool isKilled, DropCoinEvent dropCoinEvent) = connectedClient.Hit(enemy, bullet);



            if (isKilled)
            {
                connectedClient.Account.OnCoinDrop(dropCoinEvent);

                _accountService.Save(connectedClient.Account);

                // broadcast kill event
                var killEvent = new Event<EventPayload.TargetKilled>()
                {
                    EventType = (int)EventType.TargetKilled,
                    Payload = new TargetKilled()
                    {
                        KilledByName = connectedClient.Account.Name,
                        KilledBy = connectedClient.Account.ID,
                        Slot = connectedClient.Slot,
                        KilledEnemyInGameId = hitEnemy.InGameId
                    }
                };
                Broadcast("", JsonConvert.SerializeObject(killEvent));

                // TODO: broadcast dropcoint event.
                //var 
            }
        }

        public void RotationChange(IWebSocketConnection socket, string message)
        {
            var command = JsonConvert.DeserializeObject<Command<RotationChange>>(message);

            ConnectedClient connectedClient;
            if(!connectedClients.TryGetValue(socket.ConnectionInfo.Id, out connectedClient))
            {
                throw new ConnectionNotFoundException(socket.ConnectionInfo.Id);
            }

            connectedClient.RotationChange(command.Payload.Rotation);

            var rotationChanged = new Event<EventPayload.RotationChanged>()
            {
                EventType = (int)EventType.RotationChanged,
                Payload = new EventPayload.RotationChanged()
                {
                    PlayerId = connectedClient.Player.ID,
                    Rotation = command.Payload.Rotation

                }
            };

            Broadcast(connectedClient.Socket.ConnectionInfo.Id, JsonConvert.SerializeObject(rotationChanged));
        }

        public void GetRecentWorldState(IWebSocketConnection socket, string message)
        {
            ConnectedClient connectedClient;
            if (!connectedClients.TryGetValue(socket.ConnectionInfo.Id, out connectedClient))
            {
                throw new ConnectionNotFoundException(socket.ConnectionInfo.Id);
            }

            List<PlayerInfo> players = new List<PlayerInfo>();

            foreach(var client in connectedClients.Values)
            {
                players.Add(new PlayerInfo()
                {
                    ID = client.Player.ID,
                    Name = client.Account.Name,
                    Slot = client.Slot,
                    Money = new Money(
                        client.Account.Wallet.Currency,
                        client.Account.Wallet.Amount,
                        client.Account.Wallet.Precise
                        ),
                    Body = new Body()
                    {
                        Rotation = client.Rotation
                    }
                });
            }

            var gotRecentWorldState = new Event<EventPayload.GotRecentWorldState>()
            {
                EventType = (int)EventType.GotRecentWorldState,
                Payload = new GotRecentWorldState()
                {
                    PlayersInfo = players,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                }
            };

            connectedClient.Socket.Send(JsonConvert.SerializeObject(gotRecentWorldState));


        }

        // TODO: add bulletRepository.
        private Cost GetBulletCost(string bulletID)
        {
            return new Cost(
                currency: Currency.CNY,
                amount: 100,
                precise: 100
            );
        }
       
        private void Broadcast<TPayload>(Guid id, EventType eventType, TPayload payload)
        {
            var event2Send = new Event<TPayload>()
            {
                EventType = (int)eventType,
                Payload = payload
            };
            Broadcast(id, JsonConvert.SerializeObject(event2Send));
        }

        private void Broadcast(Guid id, string message)
        {
            Broadcast(id.ToString(), message);
        }

        private void Broadcast(string id, string message)
        {
            foreach (var connectedClient in connectedClients.Values)
            {
                if (connectedClient.Socket.ConnectionInfo.Id.ToString() != id)
                {
                    connectedClient.Socket.Send(message);
                }
            }
        }

        public class ConnectedClient
        {
            public IWebSocketConnection Socket { get;}
            public Player Player { get; }

            public Account Account { get; }
            public int Slot { get;  }
            private ShootThrottle _shootThrottle;
            public double Rotation { get; private set; } = 0;

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
                _shootThrottle = new ShootThrottle(this);
            }
            public void Shoot()
            {

                _shootThrottle.Increase();
                Account.OnShootEvent(
                new ShootEvent(
                    shootBy: new ShootBy(
                        account: Account,
                        player: Player
                        ),
                    bulletName: "0",
                    cost: new Cost(
                        currency: Account.Wallet.Currency,
                        amount: 100,
                        precise: Account.Wallet.Precise
                        )
                    )
                );
            }
            public void RotationChange(double value)
            {
                Rotation = value;
            }

            public (bool, DropCoinEvent) Hit(Domain.Entity.Enemy enemy, Bullet bullet)
            {
                return Player.OnHitEvent(new HitEvent(
                    hitBy: new HitBy(Player),
                    enemy: enemy,
                    bullet: bullet
                ), 50);
            }
        }

        class ShootThrottle
        {
            private object _shootLock = new object();

            private static TimeSpan Interval = TimeSpan.FromSeconds(1);
            private int Rate = 100;
            private DateTime timestamp = DateTime.UtcNow;
            private int Counter { get; set; } = 0;
            private ConnectedClient _client { get; }
            public ShootThrottle(ConnectedClient client)
            {
                _client = client;
            }

            public ShootThrottle Increase()
            {
                lock (_shootLock)
                {
                    if(timestamp + Interval < DateTime.UtcNow)
                    {
                        return new ShootThrottle(_client);
                    }

                    
                    Counter += 1;
                    if (Counter > Rate)
                    {
                        throw new ShootRateOverThrottleException(_client.Socket.ConnectionInfo.Id);
                    }

                    return this;
                }
            }
        }

    
    }
}
