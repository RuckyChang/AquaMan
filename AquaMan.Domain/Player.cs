using AquaMan.Domain.Entity;
using AquaMan.Domain.GameEvent;
using System;

namespace AquaMan.Domain
{
    public enum State
    {
        NORMAL,
        FEVER,
    }

    public class Player
    {
        public string AccountId { get;  }
        public State State { get;  }
        public string CurrentGameId { get;  }

        public Player(string accountId, State state, string gameId)
        {
            AccountId = accountId;
            State = state;
            CurrentGameId = gameId;
        }


        public (bool, DropCoinEvent) OnHitEvent(HitEvent e)
        {
            // TODO: extract this to another module.            

            var rand = new Random();

            var randResult = rand.Next(100);

            if(randResult >= 50)
            {
                return (true, new DropCoinEvent(
                    new Cost(
                        currency:  e.Bullet.Price.Currency,
                        amount: e.Enemy.RewardMoney[0].Amount,
                        precise: e.Bullet.Price.Precise
                        )
                    ));
            }

            return (false, null);
        }
    }
}
