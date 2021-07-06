using System;

namespace AquaMan.Domain
{
    public enum OrderType{
        DEPOSIT,
        WITHDRAW
    }

    public enum OrderState
    {
        PENDDING,
        LOCAL_COMMITTED,
        REMOTE_COMMITTED
    }

    public class Order
    {
        public string ID { get; set; }
        public string GameId { get; set; }
        public string AccountId { get; set; }
        public uint Amount { get; set; }
        public OrderType OrderType { get; set; }
        public DateTime OccurredAt { get; set; }
        public OrderState State { get; set; }
    }
}
