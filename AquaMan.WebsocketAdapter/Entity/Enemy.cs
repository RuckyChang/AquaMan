using System.Collections.Generic;

namespace AquaMan.WebsocketAdapter.Entity
{
    public class Enemy
    {
        public List<PatrolPoint> PatrolPoints;
        public Point RespawnPoint;

        public bool Active { get; private set; }
        public long ReapwnAt;

        public Enemy(Point respawnPoint, List<PatrolPoint> patrolPoints)
        {
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
    }
}
