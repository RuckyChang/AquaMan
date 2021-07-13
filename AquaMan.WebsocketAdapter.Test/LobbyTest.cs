using AquaMan.Domain.Entity;
using AquaMan.DomainApi;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
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

        
    }
}
