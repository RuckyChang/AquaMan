using AquaMan.Domain.Entity;

namespace AquaMan.Domain.GameEvent
{
    public class HitBy
    {
        public Account Account { get; }
        public Player Player { get; }
    }
    public class Bullet
    {
        public string ID { get; }

        public Cost Price { get; }
    }

    public class HitEvent: Event
    {
        public HitBy HitBy { get; }
        public Enemy Enemy { get; }
        public Bullet Bullet { get; }
        public HitEvent(
            HitBy hitBy,
            Enemy enemy,
            Bullet bullet
            )
        {
            HitBy = hitBy;
            Enemy = enemy;
            Bullet = bullet;
        }
    }
}
