﻿using System;
using System.Collections.Concurrent;
using AquaMan.Domain;
using AquaMan.DomainApi;
using Fleck;
using Newtonsoft.Json;

namespace AquaMan.WebsocketAdapter
{
    public partial class Lobby
    {
        WebSocketServer _server;
        ConcurrentDictionary<Guid, IWebSocketConnection> sockets = new ConcurrentDictionary<Guid, IWebSocketConnection>();
        private AccountService _accountService;

        public Lobby(AccountService accountService, int port = 8081)
        {
            _server = new WebSocketServer("ws://0.0.0.0:"+port);
            _accountService = accountService;
        }

        public void Start()
        {
            _server.Start(socket => {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    if (!sockets.TryAdd(socket.ConnectionInfo.Id, socket))
                    {
                        Console.WriteLine(string.Format("register Socket failed, {0}", socket.ConnectionInfo.Id));
                        socket.Send("register Socket failed");
                        socket.Close();
                    }
                };
                socket.OnClose = () => Console.WriteLine("Close!");
                socket.OnMessage = message =>
                {
                    
                    Console.WriteLine(message);
                    // parse message
                    ParseMessage(socket, message);
                };
            });
        }

        public void ParseMessage(IWebSocketConnection socket, string message)
        {
            var command = JsonConvert.DeserializeObject<Command>(message);
            switch ((CommandType)command.CommandType)
            {
                case CommandType.Login:
                   

                    break;
                case CommandType.Logout:
                    Command<Logout> logoutCommand = JsonConvert.DeserializeObject<Command<Logout>>(message);

                    var account = _accountService.OfToken(logoutCommand.Payload.Token);
                    account.Logout();


                    socket.Send(JsonConvert.SerializeObject(new Event<LoggedOut>()
                    {
                        Payload = new LoggedOut()
                    }));
                    break;
            }
        }

        private void Login(IWebSocketConnection socket, string message)
        {
            Command<Login> loginCommand = JsonConvert.DeserializeObject<Command<Login>>(message);

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

            if (account.Token == null || account.Token != string.Empty)
            {
                socket.Send(JsonConvert.SerializeObject(new Event<LoggedIn>()
                {
                    Payload = new LoggedIn()
                    {
                        Token = account.Token
                    }
                }));
                return;
            }

            var token = account.Login(payload.Name, payload.Password);
            socket.Send(JsonConvert.SerializeObject(new Event<LoggedIn>()
            {
                Payload = new LoggedIn()
                {
                    Token = token
                }
            }));
        }
    }
}
