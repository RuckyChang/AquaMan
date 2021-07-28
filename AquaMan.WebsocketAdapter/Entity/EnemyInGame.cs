using System.Collections.Generic;

namespace AquaMan.WebsocketAdapter.Entity
{
    public class EnemyInGame
    {
        public List<PatrolPoint> PatrolPoints;
        public Point RespawnPoint;


        public bool Active { get; private set; } = true;
        public long ReapwnAt;

        public string ID { get; }
        public string InGameId { get; }

        public EnemyInGame(string id, string inGameId, Point respawnPoint, List<PatrolPoint> patrolPoints)
        {
            ID = id;
            InGameId = inGameId;
            RespawnPoint = respawnPoint;
            PatrolPoints = patrolPoints;
        }

        public Point GetPosition(long currentTimestamp)
        {
            // TODO: iterate over Movements, and find out the current position.
            return new Point(0, 0);
        }

        public void Reset(Point respawnPoint, List<PatrolPoint> patrolPoints)
        {
            RespawnPoint = respawnPoint;
            PatrolPoints = patrolPoints;
        }

        public void Killed()
        {
            Active = false;
        }
    }
}
