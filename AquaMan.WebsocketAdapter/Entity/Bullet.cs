namespace AquaMan.WebsocketAdapter.Entity
{
    public class Coordinate
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class ShotBullet
    {
       

        public Coordinate StartFrom { get; set; }
        public Coordinate Direction { get; set; }
        public string BulletID { get; set; }
    }
}
