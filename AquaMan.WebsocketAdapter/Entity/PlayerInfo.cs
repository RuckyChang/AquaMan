namespace AquaMan.WebsocketAdapter.Entity
{
    public class Body
    {
        public double Rotation { get; set; }
    }
    public class PlayerInfo
    {
       

        public string ID { get; set; }
        public string Name { get; set; }
        public Money Money { get; set; }
        public int Slot { get; set; }
        public Body Body { get; set; }
        
    }
}
