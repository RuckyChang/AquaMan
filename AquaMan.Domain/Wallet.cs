using AquaMan.Domain.Entity;

namespace AquaMan.Domain
{
    public class Wallet
    {
        public Currency Currency { get; }
        public ulong Amount { get; private set; }
        public uint Precise { get; }

        public Wallet(Currency currency, uint amount, uint precise)
        {
            Currency = currency;
            Amount = amount;
            Precise = precise;
        }

        public bool IsSameCurrency(Cost cost)
        {
            return Currency == cost.Currency;
        }

        public bool IsEnough(Cost cost)
        {
            return Amount >= cost.Amount;
        }

        public bool Increase(Cost cost)
        {
            Amount += cost.Amount;
            return true;
        }

        public bool Decrease(Cost cost)
        {
            if(Amount < cost.Amount)
            {
                return false;
            }
            Amount -= cost.Amount;
            return true;
        }
    }
}
