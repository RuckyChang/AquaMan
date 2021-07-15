namespace AquaMan.Domain.Entity
{
    public class Bullet
    {
        public string ID { get; }

        public Cost Price { get; }

        public Bullet(string id, Cost price)
        {
            ID = id;
            Price = price;
        }
    }
}
