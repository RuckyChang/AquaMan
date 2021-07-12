﻿using AquaMan.WebsocketAdapter.Entity;

namespace AquaMan.WebsocketAdapter
{
    public partial class Lobby
    {
        public enum CommandType
        {
            Login,
            Logout
        }

        public class Login
        {
            public string Name { get; set; }
            public string Password { get; set; }
            public string AgentId { get; set; }
            public Money Moeny { get; set; }
        }

        public class Logout
        {

        }
    }
}
