namespace AquaMan.Domain.Entity
{
    public class Cost
    {
        public Currency Currency { get; }
        public uint Amount { get; }
        public uint Precise { get; }

        public Cost(Currency currency, uint amount, uint precise)
        {
            Currency = currency;
            Amount = amount;
            Precise = precise;
        }
    }
}
