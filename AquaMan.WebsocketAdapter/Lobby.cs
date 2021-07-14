using System;
using System.Collections.Concurrent;
using AquaMan.Domain;
using AquaMan.DomainApi;
using Fleck;
using Newtonsoft.Json;

namespace AquaMan.WebsocketAdapter
{
    public partial class Lobby
    {
        private AccountService _accountService;

        public Lobby(AccountService accountService)
        {
             
            _accountService = accountService;
        }

        public void Login(IWebSocketConnection socket, string message)
        {
            Command<CommandPayload.Login> loginCommand = JsonConvert.DeserializeObject<Command<CommandPayload.Login>>(message);

            var payload = loginCommand.Payload;

            var account = _accountService.OfAgentIdAndName(
                payload.AgentId,
                payload.Name
            );

            if (account == null)
            {
                account = _accountService.CreateAccount(
                    name: payload.Name,
                    password: payload.Password,
                    agentId: payload.AgentId,
                    new Wallet(
                        currency: payload.Money.Currency,
                        amount: payload.Money.Amount,
                        precise: payload.Money.Precise
                        )
                    );
            }

            if (account.Token != null && account.Token != string.Empty)
            {
                socket.Send(JsonConvert.SerializeObject(new Event<EventPayload.LoggedIn>()
                {
                    Payload = new EventPayload.LoggedIn()
                    {
                        Token = account.Token
                    }
                }));
                return;
            }

            var token = account.Login(payload.Name, payload.Password);
            _accountService.Save(account);

            socket.Send(JsonConvert.SerializeObject(new Event<EventPayload.LoggedIn>()
            {
                Payload = new EventPayload.LoggedIn()
                {
                    Token = token
                }
            }));
        }

        public void Logout(IWebSocketConnection socket, string message)
        {
            Command<CommandPayload.Logout> logoutCommand = JsonConvert.DeserializeObject<Command<CommandPayload.Logout>>(message);

            var account = _accountService.OfToken(logoutCommand.Payload.Token);
            account.Logout();

            _accountService.Save(account);

            socket.Send(JsonConvert.SerializeObject(new Event<EventPayload.LoggedOut>()
            {
                EventType = (int)EventType.LoggedOut,
                Payload = new EventPayload.LoggedOut()
            }));
        }

        public void ListGame(IWebSocketConnection socket, string message)
        {
            // todo: finish this.
            // mock
            // name
            // gameId
            //
        }
    }
}
