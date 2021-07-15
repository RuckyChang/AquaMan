using AquaMan.DomainApi;
using Newtonsoft.Json;
using System;
using System.Threading;
using Websocket.Client;
using Xunit;
using static AquaMan.WebsocketAdapter.EventPayload;

namespace AquaMan.WebsocketAdapter.Test
{
    public class GameRoomTest
    {


        private Startup StartServer(int port)
        {
            var accountRepo = new InMemoryAccountRepository();
            var accountService = new AccountService(accountRepo);

            var playerRepo = new InMemoryPlayerRepository();
            var playerService = new PlayerService(playerRepo);

            var startup = new Startup(
                port: port,
                accountService: accountService,
                playerService: playerService
                );

            startup.Start();

            return startup;
        }

        [Fact]
        public void JoinGame_ShouldPass()
        {
            var port = 8082;
            var startup = StartServer(port);
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent JoinGameEvent = new ManualResetEvent(false);

            string token = string.Empty;

            var url = new Uri("ws://127.0.0.1:"+ port);

            string receivedMessage = string.Empty;
            Event<JoinedGame> joinedGameEvent = null;

            using(var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happended, type: {info.Type}"));

                client.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if(eventType == 0)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }else if(eventType == 2)
                    {
                        joinedGameEvent = JsonConvert.DeserializeObject<Event<JoinedGame>>(msg.Text);
                        JoinGameEvent.Set();
                    }else if(eventType == -1)
                    {
                        var ErrorEvent = JsonConvert.DeserializeObject<Event<Error>>(msg.Text);
                        JoinGameEvent.Set();
                    }
                });

                client.Start();

                var loginCommand = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = "ricky",
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                            currency: Domain.Entity.Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };
                client.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                var JoinGameCommand = new Command<CommandPayload.JoinGame>()
                {
                    CommandType = (int)CommandType.JoinGame,
                    Payload = new CommandPayload.JoinGame()
                    {
                        Token = token
                    }
                };
                client.Send(JsonConvert.SerializeObject(JoinGameCommand));
                JoinGameEvent.WaitOne();
            }

            Assert.NotNull(joinedGameEvent);
            Assert.Equal("ricky", joinedGameEvent.Payload.Name);
            Assert.Equal(0, joinedGameEvent.Payload.Slot);
        }

        [Fact]
        public void JoinGame_ShouldReceivedError()
        {
            int port = 8083;
            var startup = StartServer(port);
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent ErrorEvent = new ManualResetEvent(false);

            string token = string.Empty;

            var url = new Uri("ws://127.0.0.1:"+ port);

            Event<Error> errorEvent = null;

            using (var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happended, type: {info.Type}"));

                client.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if (eventType == 0)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }
                    else if (eventType == -1)
                    {
                        errorEvent = JsonConvert.DeserializeObject<Event<Error>>(msg.Text);
                        ErrorEvent.Set();
                    }
                });

                client.Start();

                var loginCommand = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = "ricky",
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                            currency: Domain.Entity.Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };
                client.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                var JoinGameCommand = new Command<CommandPayload.JoinGame>()
                {
                    CommandType = (int)CommandType.JoinGame,
                    Payload = new CommandPayload.JoinGame()
                    {
                        Token = ""
                    }
                };
                client.Send(JsonConvert.SerializeObject(JoinGameCommand));
                ErrorEvent.WaitOne();
            }

            Assert.NotNull(errorEvent);
            Assert.Equal(-1, errorEvent.EventType);
        }

        [Fact]
        public void JoinGame_ShouldReceiveBroadcastMessge()
        {
            int port = 8084;
            var startup = StartServer(port);
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent JoinGameEvent = new ManualResetEvent(false);
            ManualResetEvent BroadcastJoinGameEvent = new ManualResetEvent(false);

            string token = string.Empty;

            var url = new Uri("ws://127.0.0.1:"+ port);

            Event<JoinedGame> broadcastedJoinGameEvent = null;

            // first client
            using (var client1 = new WebsocketClient(url))
            using (var client2 = new WebsocketClient(url))
            {
                client1.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client1.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happended, type: {info.Type}"));

                client1.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if (eventType == 0)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }
                    else if (eventType == 2)
                    {
                        var joinedGameEvent = JsonConvert.DeserializeObject<Event<JoinedGame>>(msg.Text);
                        if(joinedGameEvent.Payload.Name == "ricky_3")
                        {
                            broadcastedJoinGameEvent = joinedGameEvent;
                            BroadcastJoinGameEvent.Set();
                        }
                        else
                        {
                            JoinGameEvent.Set();
                        }
                    }
                });

                client1.Start();

                var loginCommand = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = "ricky_2",
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                            currency: Domain.Entity.Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };
                client1.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                var joinGameCommand = new Command<CommandPayload.JoinGame>()
                {
                    CommandType = (int)CommandType.JoinGame,
                    Payload = new CommandPayload.JoinGame()
                    {
                        Token = token
                    }
                };
                client1.Send(JsonConvert.SerializeObject(joinGameCommand));
                JoinGameEvent.WaitOne();
            
                LoginEvent.Reset();
                JoinGameEvent.Reset();

                client2.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client2.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happended, type: {info.Type}"));

                client2.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if (eventType == 0)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }else if (eventType == 2)
                    {
                        var joinedGameEvent = JsonConvert.DeserializeObject<Event<JoinedGame>>(msg.Text);
                        JoinGameEvent.Set();
                    }
                    else if (eventType == -1)
                    {
                        var errorEvent = JsonConvert.DeserializeObject<Event<Error>>(msg.Text);
                    }
                });

                client2.Start();

                loginCommand = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = "ricky_3",
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                            currency: Domain.Entity.Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };
                client2.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                joinGameCommand = new Command<CommandPayload.JoinGame>()
                {
                    CommandType = (int)CommandType.JoinGame,
                    Payload = new CommandPayload.JoinGame()
                    {
                        Token = token
                    }
                };
                client2.Send(JsonConvert.SerializeObject(joinGameCommand));
                JoinGameEvent.WaitOne();
            }

            BroadcastJoinGameEvent.WaitOne();

            Assert.NotNull(broadcastedJoinGameEvent);
            Assert.Equal("ricky_3", broadcastedJoinGameEvent.Payload.Name);
        }

        [Fact]
        public void QuitGame_ShouldPass()
        {
            int port = 8085;
            var startup = StartServer(port);
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent JoinGameEvent = new ManualResetEvent(false);
            ManualResetEvent QuitGameEvent = new ManualResetEvent(false);

            var url = new Uri("ws://127.0.0.1:"+ 8085);

            var token = string.Empty;

            Event<QuitGame> quitGameEvent = null;

            using(var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happend, type: {info.Type}"));

                client.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if (eventType == (int)EventType.LoggedIn)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }
                    else if (eventType == (int)EventType.JoinedGame)
                    {
                        var joinedGameEvent = JsonConvert.DeserializeObject<Event<JoinedGame>>(msg.Text);
                        JoinGameEvent.Set();
                    }else if(eventType == (int)EventType.QuitGame)
                    {
                        quitGameEvent = JsonConvert.DeserializeObject<Event<QuitGame>>(msg.Text);
                        QuitGameEvent.Set();
                    }
                    else if (eventType == (int)EventType.Error)
                    {
                        var errorEvent = JsonConvert.DeserializeObject<Event<Error>>(msg.Text);
                    }
                });
                client.Start();

                var loginCommand = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = "ricky_1",
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                            currency: Domain.Entity.Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };

                client.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                var joinGameCommand = new Command<CommandPayload.JoinGame>()
                {
                    CommandType = (int)CommandType.JoinGame,
                    Payload = new CommandPayload.JoinGame()
                    {
                        Token = token
                    }
                };
                client.Send(JsonConvert.SerializeObject(joinGameCommand));
                JoinGameEvent.WaitOne();

                var quitGameCommand = new Command<CommandPayload.QuitGame>()
                {
                    CommandType = (int)CommandType.QuitGame,
                    Payload = new CommandPayload.QuitGame()
                    {
                        Token = token
                    }
                };

                client.Send(JsonConvert.SerializeObject(quitGameCommand));
                QuitGameEvent.WaitOne();
            }

            Assert.Equal("ricky_1", quitGameEvent.Payload.Name);
            Assert.Equal(0, quitGameEvent.Payload.Slot);
        }

        [Fact]
        public void QuitGame_ShouldReceiveBroadcastMessage()
        {
            int port = 8086;
            var startup = StartServer(port);
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent JoinGameEvent = new ManualResetEvent(false);
            ManualResetEvent QuitGameEvent = new ManualResetEvent(false);

            var url = new Uri("ws://127.0.0.1:"+ port);

            var name1 = Guid.NewGuid().ToString();
            var name2 = Guid.NewGuid().ToString();

            var token = string.Empty;

            Event<QuitGame> quitGameEvent = null;
            // client 1 join game
            using(var client = new WebsocketClient(url))
            using(var client2= new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happend, type: {info.Type}"));

                client.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if (eventType == (int)EventType.LoggedIn)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }
                    else if (eventType == (int)EventType.JoinedGame)
                    {
                        var joinedGameEvent = JsonConvert.DeserializeObject<Event<JoinedGame>>(msg.Text);
                        JoinGameEvent.Set();
                    }
                    else if (eventType == (int)EventType.QuitGame)
                    {
                        var tmp = JsonConvert.DeserializeObject<Event<QuitGame>>(msg.Text);
                        if(tmp.Payload.Name == name2)
                        {
                            quitGameEvent = tmp;
                            QuitGameEvent.Set();
                        }
                    }
                    else if (eventType == (int)EventType.Error)
                    {
                        var errorEvent = JsonConvert.DeserializeObject<Event<Error>>(msg.Text);
                    }
                });
                client.Start();

                var loginCommand = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = name1,
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                            currency: Domain.Entity.Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };

                client.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                var joinGameCommand = new Command<CommandPayload.JoinGame>()
                {
                    CommandType = (int)CommandType.JoinGame,
                    Payload = new CommandPayload.JoinGame()
                    {
                        Token = token
                    }
                };
                client.Send(JsonConvert.SerializeObject(joinGameCommand));
                JoinGameEvent.WaitOne();

                LoginEvent.Reset();
                JoinGameEvent.Reset();

                client2.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client2.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happend, type: {info.Type}"));

                client2.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if (eventType == (int)EventType.LoggedIn)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }
                    else if (eventType == (int)EventType.JoinedGame)
                    {
                        var joinedGameEvent = JsonConvert.DeserializeObject<Event<JoinedGame>>(msg.Text);
                        JoinGameEvent.Set();
                    }
                    else if (eventType == (int)EventType.QuitGame)
                    {
                        
                    }
                    else if (eventType == (int)EventType.Error)
                    {
                        var errorEvent = JsonConvert.DeserializeObject<Event<Error>>(msg.Text);
                    }
                });
                client2.Start();
                loginCommand = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = name2,
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                          currency: Domain.Entity.Currency.CNY,
                          amount: 10000,
                          precise: 100
                          )
                    }
                };

                client2.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                joinGameCommand = new Command<CommandPayload.JoinGame>()
                {
                    CommandType = (int)CommandType.JoinGame,
                    Payload = new CommandPayload.JoinGame()
                    {
                        Token = token
                    }
                };
                client2.Send(JsonConvert.SerializeObject(joinGameCommand));
                JoinGameEvent.WaitOne();

                var quitGameCommand = new Command<CommandPayload.QuitGame>()
                {
                    CommandType = (int)CommandType.QuitGame,
                    Payload = new CommandPayload.QuitGame()
                    {
                        Token = token
                    }
                };

                client2.Send(JsonConvert.SerializeObject(quitGameCommand));
                QuitGameEvent.WaitOne();
            }

            Assert.NotNull(quitGameEvent);
            Assert.Equal(name2, quitGameEvent.Payload.Name);
        }

        [Fact]
        public void Shoot_ShouldPass()
        {
            var port = 8087;
            var startup = StartServer(port);
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent JoinGameEvent = new ManualResetEvent(false);
            ManualResetEvent ShotEvent = new ManualResetEvent(false);

            string token = string.Empty;

            var url = new Uri("ws://127.0.0.1:" + port);


            Event<Shot> shotEvent = null;

            var startFrom = new Entity.ShotBullet.Coordinate()
            {
                X = 0,
                Y = 0,
                Z = 0,
            };
            var direction = new Entity.ShotBullet.Coordinate()
            {
                X = 1,
                Y = 0,
                Z = 0,
            };

            using (var client = new WebsocketClient(url))
            {
                client.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if (eventType == (int)EventType.LoggedIn)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }
                    else if (eventType == (int)EventType.JoinedGame)
                    {
                        var joinedGameEvent = JsonConvert.DeserializeObject<Event<JoinedGame>>(msg.Text);
                        JoinGameEvent.Set();
                    }
                    else if (eventType == (int)EventType.Shot)
                    {
                        shotEvent = JsonConvert.DeserializeObject<Event<Shot>>(msg.Text);
                        ShotEvent.Set();
                    }
                    else if (eventType == (int)EventType.Error)
                    {
                        var errorEvent = JsonConvert.DeserializeObject<Event<Error>>(msg.Text);
                        ShotEvent.Set();
                    }
                });
                client.Start();

                var loginCommand = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = "ricky",
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                            currency: Domain.Entity.Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };

                client.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                var joinGameCommand = new Command<CommandPayload.JoinGame>()
                {
                    CommandType = (int)CommandType.JoinGame,
                    Payload = new CommandPayload.JoinGame()
                    {
                        Token = token
                    }
                };
                client.Send(JsonConvert.SerializeObject(joinGameCommand));
                JoinGameEvent.WaitOne();

               

                var shootCommand = new Command<CommandPayload.Shoot>()
                {
                    CommandType = (int)CommandType.Shoot,
                    Payload = new CommandPayload.Shoot()
                    {
                        Token = token,
                        ShotBullet = new Entity.ShotBullet()
                        {
                            StartFrom = startFrom,
                            Direction = direction,
                            BulletID = "0"
                        }
                    }
                };

                client.Send(JsonConvert.SerializeObject(shootCommand));
                ShotEvent.WaitOne();
            }

            var payload = shotEvent.Payload;

            Assert.Equal("ricky", payload.Shooter);
            Assert.Equal(0, payload.Slot);

            var shotBullet = payload.ShotBullet;
            Assert.Equal("0", shotBullet.BulletID);
            Assert.Equal(startFrom.X, shotBullet.StartFrom.X);
            Assert.Equal(startFrom.Y, shotBullet.StartFrom.Y);
            Assert.Equal(startFrom.Z, shotBullet.StartFrom.Z);

            Assert.Equal(direction.X, shotBullet.Direction.X);
            Assert.Equal(direction.Y, shotBullet.Direction.Y);
            Assert.Equal(direction.Z, shotBullet.Direction.Z);
        }

        [Fact]
        public void Shoot_ShouldReceivedBroadcastMessage()
        {
            var port = 8088;
            var startup = StartServer(port);
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent JoinGameEvent = new ManualResetEvent(false);
            ManualResetEvent ShotEvent = new ManualResetEvent(false);
            ManualResetEvent BroadcastShotEvent = new ManualResetEvent(false);

            string token = string.Empty;

            var url = new Uri("ws://127.0.0.1:" + port);


            Event<Shot> shotEvent = null;

            var startFrom = new Entity.ShotBullet.Coordinate()
            {
                X = 0,
                Y = 0,
                Z = 0,
            };
            var direction = new Entity.ShotBullet.Coordinate()
            {
                X = 1,
                Y = 0,
                Z = 0,
            };

            var clientName_1 = Guid.NewGuid().ToString();
            var clientName_2 = Guid.NewGuid().ToString();

            using(var client = new WebsocketClient(url))
            using(var client2 = new WebsocketClient(url))
            {
                client.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if (eventType == (int)EventType.LoggedIn)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }
                    else if (eventType == (int)EventType.JoinedGame)
                    {
                        var joinedGameEvent = JsonConvert.DeserializeObject<Event<JoinedGame>>(msg.Text);
                        JoinGameEvent.Set();
                    }
                    else if (eventType == (int)EventType.Shot)
                    {
                        
                        var tmp = JsonConvert.DeserializeObject<Event<Shot>>(msg.Text);
                        if(tmp.Payload.Shooter == )
                        ShotEvent.Set();
                    }
                    else if (eventType == (int)EventType.Error)
                    {
                        var errorEvent = JsonConvert.DeserializeObject<Event<Error>>(msg.Text);
                        ShotEvent.Set();
                    }
                });
                client.Start();

                var loginCommand = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = clientName_1,
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                            currency: Domain.Entity.Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };

                client.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                var joinGameCommand = new Command<CommandPayload.JoinGame>()
                {
                    CommandType = (int)CommandType.JoinGame,
                    Payload = new CommandPayload.JoinGame()
                    {
                        Token = token
                    }
                };
                client.Send(JsonConvert.SerializeObject(joinGameCommand));
                JoinGameEvent.WaitOne();

                // TODO: client2 init.
                client2.MessageReceived.Subscribe(msg =>
                {
                    var eventType = Utils.ParseEventType(msg.Text);

                    if (eventType == (int)EventType.LoggedIn)
                    {
                        var loginEvent = JsonConvert.DeserializeObject<Event<LoggedIn>>(msg.Text);
                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }
                    else if (eventType == (int)EventType.JoinedGame)
                    {
                        var joinedGameEvent = JsonConvert.DeserializeObject<Event<JoinedGame>>(msg.Text);
                        JoinGameEvent.Set();
                    }
                    else if (eventType == (int)EventType.Shot)
                    {
                        shotEvent = JsonConvert.DeserializeObject<Event<Shot>>(msg.Text);
                        ShotEvent.Set();
                    }
                    else if (eventType == (int)EventType.Error)
                    {
                        var errorEvent = JsonConvert.DeserializeObject<Event<Error>>(msg.Text);
                        ShotEvent.Set();
                    }
                });


            }



        }
    }
}
