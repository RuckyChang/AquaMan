using AquaMan.Domain.Entity;
using AquaMan.Domain.GameEvent;
using System;

namespace AquaMan.Domain
{
    public class Account
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Agent { get; set; }
        public string Token { get; set; }//TODO: maybe use TTL for this?
        public string LastLoginTime { get; set; }
        public Wallet Wallet { get; set; }

        public void Login(string name, string password)
        {
            if(name != Name || Password != password)
            {
                throw new LoginFailedException(name, password);
            }
        }
        public void Deposit(Cost cost)
        {
            if (!Wallet.IsSameCurrency(cost))
            {
                throw new CurrencyNotMatchException(
                    accountID: ID,
                    wallet: Wallet,
                    cost: cost
                    );
            }

            Wallet.Increase(cost);
        }

        public void Withdraw(Cost cost)
        {
            if(!Wallet.IsSameCurrency(cost))
            {
                throw new CurrencyNotMatchException(
                   accountID: ID,
                   wallet: Wallet,
                   cost: cost
                   );
            }

            if(!Wallet.IsEnough(cost))
            {
                throw new NotEnoughAmountException(
                    accountID: ID,
                    wallet: Wallet,
                    cost: cost
                    );
            }

            Wallet.Decrease(cost);
        }

        public void OnShootEvent(ShootEvent e)
        {
            if(!Wallet.IsSameCurrency(e.Cost))
            {
                throw new CurrencyNotMatchException(
                    accountID: ID,
                    wallet: Wallet,
                    cost: e.Cost
                    );
            }

            // deduct money.
            if (!Wallet.IsEnough(e.Cost))
            {
                throw new NotEnoughAmountException(
                   accountID: ID,
                   wallet: Wallet,
                   cost: e.Cost
                   );
            }

            Wallet.Decrease(e.Cost);
        }
        public void OnCoinDrop(DropCoinEvent e)
        {
            if(!Wallet.IsSameCurrency(e.Coin))
            {
                throw new CurrencyNotMatchException(
                                    accountID: ID,
                                   wallet: Wallet,
                                   cost: e.Coin
                                   );
            }

            Wallet.Increase(e.Coin);
        }
    }

    public class LoginFailedException: Exception
    {
        public string Name { get;  }
        public string Password { get;  }

        public LoginFailedException(string name, string password): base()
        {
            Name = name;
            Password = password;
        }
    }

    public class CurrencyNotMatchException : Exception
    {
        public string AccountID { get; }

        public Wallet Wallet { get; }

        public Cost Cost { get; }

        public CurrencyNotMatchException(
            string accountID, 
            Wallet wallet, 
            Cost cost
            ) : base() {
            AccountID = accountID;
            Wallet = wallet;
            Cost = cost;
        }
    }

    public class NotEnoughAmountException: Exception
    {
        public string AccountID { get; }
        public Wallet Wallet { get; }
        public Cost Cost { get; }

        public NotEnoughAmountException(
            string accountID, 
            Wallet wallet, 
            Cost cost
            ): base()
        {
            AccountID = accountID;
            Wallet = wallet;
            Cost = cost;
        }
    }
}
