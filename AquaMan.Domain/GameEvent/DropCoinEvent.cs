using AquaMan.Domain.Entity;

namespace AquaMan.Domain.GameEvent
{
    public class DropCoinEvent: Event
    {
        public Cost Coin { get; set; }

        public DropCoinEvent(Cost coin)
        {
            Coin = coin;
        }
    }
}
