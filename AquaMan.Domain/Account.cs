using AquaMan.Domain.Entity;
using AquaMan.Domain.GameEvent;
using System;

namespace AquaMan.Domain
{
    public class Account
    {
        public string ID { get; }
        public string Name { get; }
        public string Password { get; }
        public string AgentId { get; }
        public string Token { get; private set; } = "";
        public DateTime? LastLoginTime { get; private set; }
        public Wallet Wallet { get; }
        public Account(
            string id,
            string name,
            string password,
            string agentId,
            string token,
            DateTime? lastLoginTime,
            Wallet wallet
            )
        {
            ID = id;
            Name = name;
            Password = password;
            AgentId = agentId;
            Token = token;
            LastLoginTime = lastLoginTime;
            Wallet = wallet;
        }

        public string Login(string name, string password)
        {
            if(name != Name || Password != password)
            {
                throw new LoginFailedException(name, password);
            }

            Token = Guid.NewGuid().ToString();
            LastLoginTime = DateTime.Now;

            return Token;
        }

        public void Logout()
        {
            Token = null;
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
