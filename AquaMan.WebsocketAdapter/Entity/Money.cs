using AquaMan.Domain.Entity;


namespace AquaMan.WebsocketAdapter.Entity
{
    public class Money
    {
        public Currency Currency { get; }
        public uint Amount { get; private set; }
        public uint Precise { get;  }

        public Money(Currency currency, uint amount, uint precise)
        {
            Currency = currency;
            Amount = amount;
            Precise = precise;
        }
    }
}
