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
        Shoot,
        HitTarget,
        RotationChange,
        GetRecentWorldState
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
        public class JoinGame: AuthorizedPayload{
            public string RoomId { get; set; }
        }

        public class QuitGame: AuthorizedPayload {}
        public class Shoot: AuthorizedPayload { 
            public ShotBullet ShotBullet { get; set; }
        }

        public class HitTarget : AuthorizedPayload
        {
            public string HitBy { get; set; }
            public string InGameId { get; set; }
        }

        public class RotationChange: AuthorizedPayload
        {
            public double Rotation { get; set; }
        }

        public class GetRecentWorldState: AuthorizedPayload
        {

        }
        #endregion
    }
}
