using AquaMan.Domain.Entity;
using System;
using System.Collections.Generic;

namespace AquaMan.Domain.Entity
{
    public class RewardMoney
    {
        public Currency Currency { get; }
        public uint Amount { get; }

        public uint Precise { get; }
        public RewardMoney(Currency currency, uint amount, uint precise)
        {
            Currency = currency;
            Amount = amount;
            Precise = precise;
        }
    }

    public class Enemy
    {
        public string ID { get; }
        public List<RewardMoney> RewardMoney { get; } = new List<RewardMoney>();

        public Enemy()
        {
            ID = new Guid().ToString();
            RewardMoney.Add(new RewardMoney(
                currency: Currency.USD,
                amount: 100,
                precise: 100
            ));
        }
    }
}
