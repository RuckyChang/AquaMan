using AquaMan.Domain.Entity;


namespace AquaMan.WebsocketAdapter.Command
{
    public class Money
    {
        public Currency Currency { get; }
        public ulong Amount { get; private set; }
        public uint Precise { get;  }

        public Money(Currency currency, ulong amount, uint precise)
        {
            Currency = currency;
            Amount = amount;
            Precise = precise;
        }
    }
}
