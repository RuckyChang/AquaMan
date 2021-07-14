using AquaMan.Domain.Entity;
using AquaMan.DomainApi;
using Newtonsoft.Json;
using System;
using System.Threading;
using Websocket.Client;
using Xunit;

namespace AquaMan.WebsocketAdapter.Test
{
    public class LobbyTest
    {
        private Startup _startup;
        public LobbyTest()
        {
            var accountRepo = new InMemoryAccountRepository();
            var accountService = new AccountService(accountRepo);

            var playerRepo = new InMemoryPlayerRepository();
            var playerService = new PlayerService(playerRepo);

            _startup = new Startup(
                port: 8081,
                accountService: accountService,
                playerService: playerService
                );

            _startup.Start();
        }

        [Fact]
        public void Login_ShouldPass()
        {
            ManualResetEvent ExitEvent = new ManualResetEvent(false);

            var url = new Uri("ws://127.0.0.1:8081");

            string receivedMessage = string.Empty;

            using(var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happend, type: {info.Type}"));

                client.MessageReceived.Subscribe(msg => { 
                    receivedMessage = msg.Text;
                    ExitEvent.Set();
                } );
                client.Start();

                var command = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login() {
                        Name = "ricky",
                        Password = "123456",
                        AgentId = "agentId_1",
                        Money = new Entity.Money(
                            currency: Currency.CNY, 
                            amount: 10000, 
                            precise: 100
                            )
                    }
                };

                client.Send(JsonConvert.SerializeObject(command));
                ExitEvent.WaitOne();
            }

            Assert.NotEmpty(receivedMessage);

            var loginEvent = JsonConvert.DeserializeObject<Event<EventPayload.LoggedIn>>(receivedMessage);

            Assert.NotEmpty(loginEvent.Payload.Token);
        }

        [Theory]
        [InlineData("" ,"12345", "agentId_1")]
        [InlineData("ricky", "", "agentId_1")]
        [InlineData("ricky", "12345", "")]
        public void Login_ShouldNotPass(string name, string password, string agentId)
        {
            ManualResetEvent ExitEvent = new ManualResetEvent(false);

            var url = new Uri("ws://127.0.0.1:8081");

            string receivedMessage = string.Empty;

            using (var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happend, type: {info.Type}"));

                client.MessageReceived.Subscribe(msg => {
                    receivedMessage = msg.Text;
                    ExitEvent.Set();
                });
                client.Start();

                var command = new Command<CommandPayload.Login>()
                {
                    CommandType = (int)CommandType.Login,
                    Payload = new CommandPayload.Login()
                    {
                        Name = name,
                        Password = password,
                        AgentId = agentId,
                        Money = new Entity.Money(
                            currency: Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };

                client.Send(JsonConvert.SerializeObject(command));
                ExitEvent.WaitOne();
            }

            Assert.NotEmpty(receivedMessage);

            var errorEvent = JsonConvert.DeserializeObject<Event<EventPayload.Error>>(receivedMessage);

            Assert.Equal(-1, errorEvent.EventType);
            Assert.NotEmpty(errorEvent.Payload.ErrorCode);
        }

        [Fact]
        public void Logout_ShouldPass()
        {
            ManualResetEvent ExitEvent = new ManualResetEvent(false);
            ManualResetEvent LoginEvent = new ManualResetEvent(false);

            var accountRepo = new InMemoryAccountRepository();
            var accountService = new AccountService(accountRepo);

            var url = new Uri("ws://127.0.0.1:8081");

            string receivedMessage = string .Empty;
            string token = string.Empty;

            using (var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happend, type: {info.Type}"));

                client.MessageReceived.Subscribe(msg => {

                    var eventType = Utils.ParseEventType(msg.Text);
                    if(eventType == 0)
                    {
                        //parse token out.
                        var loginEvent = JsonConvert.DeserializeObject<Event<EventPayload.LoggedIn>>(msg.Text);

                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }else if(eventType == 1)
                    {
                        receivedMessage = msg.Text;
                        ExitEvent.Set();
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
                            currency: Currency.CNY,
                            amount: 10000,
                            precise: 100
                            )
                    }
                };

                client.Send(JsonConvert.SerializeObject(loginCommand));
                LoginEvent.WaitOne();

                var logoutCommand = new Command<CommandPayload.Logout>()
                {
                    CommandType = (int)CommandType.Logout,
                    Payload = new CommandPayload.Logout()
                    {
                        Token = token
                    }
                };

                client.Send(JsonConvert.SerializeObject(logoutCommand));
                ExitEvent.WaitOne();
            }

            Assert.NotEmpty(receivedMessage);

            var logoutEvent = JsonConvert.DeserializeObject<Event<EventPayload.LoggedOut>>(receivedMessage);

            Assert.Equal(1, logoutEvent.EventType);
        }

     
    }
}
