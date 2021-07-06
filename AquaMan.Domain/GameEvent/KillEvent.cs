using AquaMan.Domain.Entity;

namespace AquaMan.Domain.GameEvent
{
    public class KilledBy
    {
        public Account Account { get; }
        public Player Player { get; }
    }

    public class KillEvent: Event
    {
        public KilledBy KilledBy { get; }

        public Enemy Enemy { get; }

        public KillEvent(KilledBy killedBy, Enemy enemy)
        {
            Enemy = enemy;
            KilledBy = killedBy;
        }
    }
}
