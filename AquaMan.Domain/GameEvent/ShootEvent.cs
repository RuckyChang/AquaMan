using AquaMan.Domain.Entity;

namespace AquaMan.Domain.GameEvent
{

    public class ShootBy
    {
        public Account Account { get;  }
        public Player Player { get; }

        public ShootBy(Account account, Player player)
        {
            Account = account;
            Player = player;
        }
    }

    public class ShootEvent: Event
    {
        public ShootBy ShootBy { get; }
        public string BulletName { get; }
        public Cost Cost { get; }

        public ShootEvent(ShootBy shootBy, string bulletName, Cost cost): base()
        {
            ShootBy = shootBy;            
            BulletName = bulletName;
            Cost = cost;
        }
    }
}
