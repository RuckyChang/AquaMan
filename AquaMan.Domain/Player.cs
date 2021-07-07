using AquaMan.Domain.Entity;
using AquaMan.Domain.GameEvent;
using System;

namespace AquaMan.Domain
{
    public enum PlayerState
    {
        NotInGame,
        InGame,
    }

    public class Player
    {
        public string AccountId { get;  }

        public PlayerState State
        {
            get
            {
                return CurrentGameId == string.Empty ? PlayerState.NotInGame : PlayerState.InGame;
            }
        }
        public string CurrentGameId { get; private set; }

        public Player(string accountId, string gameId)
        {
            AccountId = accountId;
            CurrentGameId = gameId;
        }

        public void OnJoinGame(string gameIdToJoin)
        {
            if (CurrentGameId != string.Empty)
            {
                throw new PlayerAlreadyInGameException(CurrentGameId, gameIdToJoin);
            }

            CurrentGameId = gameIdToJoin;
        }

        public void OnQuitGame(string gameIdToQuit)
        {
            if(CurrentGameId != gameIdToQuit)
            {
                throw new PlayerNotInTheGameException(CurrentGameId, gameIdToQuit);
            }

            CurrentGameId = string.Empty;
        }

        public (bool, DropCoinEvent) OnHitEvent(HitEvent e, int killPossibility)
        {

            // TODO: extract this to another module.            

            var rand = new Random();

            var randResult = rand.Next(100) + 1;

            if(randResult <= killPossibility)
            {
                return (true, new DropCoinEvent(
                    new Cost(
                        currency:  e.Bullet.Price.Currency,
                        amount: e.Enemy.RewardMoney[0].Amount,
                        precise: e.Enemy.RewardMoney[0].Precise
                        )
                    ));
            }

            return (false, null);
        }
    }

    public class PlayerAlreadyInGameException: Exception
    {
        public string CurrentGameId { get; }
        public string GameIdToJoin { get; }
        public PlayerAlreadyInGameException(string currentGameId, string gameToJoin)
        {
            CurrentGameId = currentGameId;
            GameIdToJoin = gameToJoin;
        }
    }

    public class PlayerNotInTheGameException: Exception
    {
        public string CurrentGameId { get; }
        public string GameIdToQuit { get; }
        public PlayerNotInTheGameException(string currentGameId, string gameIdToQuit)
        {
            CurrentGameId = currentGameId;
            GameIdToQuit = gameIdToQuit;
        }
    }
}
