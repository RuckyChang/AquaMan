using AquaMan.Domain.GameEvent;

namespace AquaMan.WebsocketAdapter.Entity
{
    public class ShotBullet
    {
        public class Coordinate
        {
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }
        }
        public Coordinate StartFrom { get; set; }
        public Coordinate Direction { get; set; }
        public Bullet Bullet { get; set; }
    }
}
