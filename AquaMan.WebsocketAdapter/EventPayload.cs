using AquaMan.Domain.Entity;
using AquaMan.WebsocketAdapter.Entity;

namespace AquaMan.WebsocketAdapter
{
    public enum EventType
    {
        Error = -1,
        LoggedIn,
        LoggedOut,
        JoinedGame,
        QuitGame,
        Shot,
        TargetKilled
    }

    public class EventPayload
    {
        public class Error
        {
            public string ErrorCode { get; set; }
            public string ErrorMessage { get; set; }
        }
        #region lobby
        public class LoggedIn
        {
            public string Token { get; set; }
        }

        public class LoggedOut
        {

        }
        #endregion

        #region game
        public class JoinedGame
        {
            public string RoomId { get; set; }
            public string Name { get; set; }
            public int Slot { get; set; }
        }

        public class QuitGame
        {
            public string Name { get; set; }
            public int Slot { get; set; }
        }
        public class Shot
        {
            public string Shooter { get; set; }
            public int Slot { get; set; }
            public ShotBullet ShotBullet { get; set;}
        }

        public class TargetKilled
        {
            public string KilledBy { get; set; }
            public int Slot { get; set; }
            public Cost cost { get; set; }
        }
        #endregion
    }
}
