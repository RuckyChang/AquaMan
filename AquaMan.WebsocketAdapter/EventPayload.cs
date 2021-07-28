using AquaMan.Domain.Entity;
using AquaMan.WebsocketAdapter.Entity;
using System.Collections.Generic;

namespace AquaMan.WebsocketAdapter
{
    public enum EventType
    {
        Error = -1,
        LoggedIn,
        LoggedOut,
        JoinedGame,
        PlayerQuitGame,
        Shot,
        TargetKilled,
        RotationChanged,
        GotRecentWorldState,
        PlayerJoinedGame,
        RespawnedEnemies,
        DroppedCoin,
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
            public string ID { get; set; }
            public string RoomId { get; set; }
            public string Name { get; set; }
            public int Slot { get; set; }
        }

        public class PlayerQuitGame
        {
            public string ID { get; set; }
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
            public string KilledByName { get; set; }
            public int Slot { get; set; }
            public string KilledEnemyInGameId { get; set;}
        }

        public class RotationChanged
        {
            public string PlayerId { get; set; }
            public double Rotation { get; set; }
        }

        public class GotRecentWorldState: AuthorizedPayload
        {
            public List<PlayerInfo> PlayersInfo { get; set; }
            public long Timestamp { get; set; }

            // add enemies.

        }

        public class PlayerJoinedGame
        {
            public PlayerInfo PlayerInfo { get; set; }
        }

        public class RespawnedEnemies
        {
            public List<Entity.EnemyInGame> Enemies { get; set; }
        }
        #endregion
    }
}
