using AquaMan.WebsocketAdapter.Entity;
using System.Collections.Generic;

namespace AquaMan.WebsocketAdapter
{
    public enum CommandType
    {
        Login,
        Logout,
        JoinGame,
        QuitGame,
    }
    
    public class CommandPayload
    {
        #region Lobby
        public class Login
        {
            public string Name { get; set; }
            public string Password { get; set; }
            public string AgentId { get; set; }
            public Money Money { get; set; }
        }

        public class Logout: AuthorizedPayload
        {
        }
        #endregion
        #region game
        public class JoinGame: AuthorizedPayload
        {

        }

        public class QuitGame: AuthorizedPayload { 
        }
        #endregion
    }
}
