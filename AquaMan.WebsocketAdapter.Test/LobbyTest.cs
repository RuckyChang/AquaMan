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
        [Fact]
        public void Login_shouldPass()
        {
            ManualResetEvent ExitEvent = new ManualResetEvent(false);

            var accountRepo = new InMemoryAccountRepository();
            var accountService = new AccountService(accountRepo);

            var lobby = new Lobby(accountService);
            lobby.Start();

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

                var command = new Command<Lobby.CommandPayload.Login>()
                {
                    CommandType = (int)Lobby.CommandType.Login,
                    Payload = new Lobby.CommandPayload.Login() {
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

            var loginEvent = JsonConvert.DeserializeObject<Event<Lobby.EventPayload.LoggedIn>>(receivedMessage);

            Assert.NotEmpty(loginEvent.Payload.Token);
        }

        [Theory]
        [InlineData("" ,"12345", "agentId_1")]
        [InlineData("ricky", "", "agentId_1")]
        [InlineData("ricky", "12345", "")]
        public void Login_ShouldNotPass(string name, string password, string agentId)
        {
            ManualResetEvent ExitEvent = new ManualResetEvent(false);

            var accountRepo = new InMemoryAccountRepository();
            var accountService = new AccountService(accountRepo);

            var lobby = new Lobby(accountService);
            lobby.Start();

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

                var command = new Command<Lobby.CommandPayload.Login>()
                {
                    CommandType = (int)Lobby.CommandType.Login,
                    Payload = new Lobby.CommandPayload.Login()
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

            var errorEvent = JsonConvert.DeserializeObject<Event<Lobby.EventPayload.Error>>(receivedMessage);

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

            var lobby = new Lobby(accountService);
            lobby.Start();

            var url = new Uri("ws://127.0.0.1:8081");

            string receivedMessage = string .Empty;
            string token = string.Empty;

            using (var client = new WebsocketClient(url))
            {
                client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                client.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happend, type: {info.Type}"));

                client.MessageReceived.Subscribe(msg => {

                    var eventType = ParseEventType(msg.Text);
                    if(eventType == 0)
                    {
                        //parse token out.
                        var loginEvent = JsonConvert.DeserializeObject<Event<Lobby.EventPayload.LoggedIn>>(msg.Text);

                        token = loginEvent.Payload.Token;
                        LoginEvent.Set();
                    }

                    if(eventType == 1)
                    {
                        receivedMessage = msg.Text;
                        ExitEvent.Set();
                    }
                });
                client.Start();

                var loginCommand = new Command<Lobby.CommandPayload.Login>()
                {
                    CommandType = (int)Lobby.CommandType.Login,
                    Payload = new Lobby.CommandPayload.Login()
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

                var logoutCommand = new Command<Lobby.CommandPayload.Logout>()
                {
                    CommandType = (int)Lobby.CommandType.Logout,
                    Payload = new Lobby.CommandPayload.Logout()
                    {
                        Token = token
                    }
                };

                client.Send(JsonConvert.SerializeObject(logoutCommand));
                ExitEvent.WaitOne();
            }

            Assert.NotEmpty(receivedMessage);

            var logoutEvent = JsonConvert.DeserializeObject<Event<Lobby.EventPayload.LoggedOut>>(receivedMessage);

            Assert.Equal(1, logoutEvent.EventType);
        }

        private int ParseEventType(string message)
        {
            var eventPkg = JsonConvert.DeserializeObject<Event>(message);

            return eventPkg.EventType;
        }
    }
}
