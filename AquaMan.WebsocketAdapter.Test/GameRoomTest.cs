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
        private Startup _startup;
        public GameRoomTest()
        {
            var accountRepo = new InMemoryAccountRepository();
            var accountService = new AccountService(accountRepo);

            var playerRepo = new InMemoryPlayerRepository();
            var playerService = new PlayerService(playerRepo);

            _startup = new Startup(
                port: 8082,
                accountService: accountService,
                playerService: playerService
                );

            _startup.Start();
        }

        [Fact]
        public void JoinGame_ShouldPass()
        {
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent JoinGameEvent = new ManualResetEvent(false);

            string token = string.Empty;

            var url = new Uri("ws://127.0.0.1:8082");

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
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent ErrorEvent = new ManualResetEvent(false);

            string token = string.Empty;

            var url = new Uri("ws://127.0.0.1:8082");

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
            ManualResetEvent LoginEvent = new ManualResetEvent(false);
            ManualResetEvent JoinGameEvent = new ManualResetEvent(false);
            ManualResetEvent BroadcastJoinGameEvent = new ManualResetEvent(false);

            string token = string.Empty;

            var url = new Uri("ws://127.0.0.1:8082");

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

    }
}
