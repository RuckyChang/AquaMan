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
        public string ID { get; }
        public string AccountId { get;  }

        public PlayerState State
        {
            get
            {
                return CurrentGameRoomId == string.Empty ? PlayerState.NotInGame : PlayerState.InGame;
            }
        }
        public string CurrentGameRoomId { get; private set; } = string.Empty;

        public Player(string id, string accountId)
        {
            ID = id;
            AccountId = accountId;
        }

        public Player(string id, string accountId, string gameRoomId)
        {
            ID = id;
            AccountId = accountId;
            CurrentGameRoomId = gameRoomId;
        }

        public void OnJoinGame(string gameIdToJoin)
        {
            if (CurrentGameRoomId != string.Empty)
            {
                throw new PlayerAlreadyInGameException(AccountId, CurrentGameRoomId, gameIdToJoin);
            }

            CurrentGameRoomId = gameIdToJoin;
        }

        public void OnQuitGame(string gameIdToQuit)
        {
            if(CurrentGameRoomId != gameIdToQuit)
            {
                throw new PlayerNotInTheGameException(CurrentGameRoomId, gameIdToQuit);
            }

            CurrentGameRoomId = string.Empty;
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
        public string AccountId { get;  }
        public string CurrentGameId { get; }
        public string GameIdToJoin { get; }
        public PlayerAlreadyInGameException(
            string accountId,
            string currentGameId, 
            string gameToJoin): 
            base ($@"currentGameId: {currentGameId}, gameToJoin:{gameToJoin}, accountId: {accountId}")
        {
            AccountId = accountId;
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
