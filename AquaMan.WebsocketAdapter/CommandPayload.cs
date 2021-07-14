using AquaMan.WebsocketAdapter.Entity;

namespace AquaMan.WebsocketAdapter
{
    public enum CommandType
    {
        Login,
        Logout,
        ListGame,
        JoinGame
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
        public class ListGame: AuthorizedPayload
        {

        }
        #endregion
        #region game
        public class JoinGame: AuthorizedPayload
        {

        }
        #endregion
    }
}
