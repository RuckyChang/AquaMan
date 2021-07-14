using AquaMan.Domain.Entity;
using AquaMan.Domain.GameEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AquaMan.Domain.Test
{
    public class PlayerUnitTest
    {
        [Fact]
        public void OnJoinGame_ShouldPass()
        {
            var player = new Player(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(), 
                "");
            player.OnJoinGame("game1");

            Assert.Equal(PlayerState.InGame, player.State);
        }

        [Theory]
        [InlineData("game1", "game2")]
        [InlineData("game1", "game1")]
        public void OnJoinGame_ShouldThrowInGameExcpetion(string currentGameId, string gameIdToJoin)
        {
            var player = new Player(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(), 
                currentGameId);

            PlayerAlreadyInGameException expected = null;

            try
            {
                player.OnJoinGame(gameIdToJoin);
            }catch(PlayerAlreadyInGameException e)
            {
                expected = e;
            }

            Assert.Equal(PlayerState.InGame, player.State);
            Assert.NotNull(expected);
            Assert.Equal(currentGameId, player.CurrentGameRoomId);
            Assert.Equal(expected.CurrentGameId, player.CurrentGameRoomId);
            Assert.Equal(expected.GameIdToJoin, gameIdToJoin);
        }

        [Theory]
        [InlineData("game1","game1")]
        public void OnQuitGame_ShouldPass(string currentGameId, string gameIdToQuit)
        {
            var player = new Player(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(), 
                currentGameId
                );

            player.OnQuitGame(gameIdToQuit);

            Assert.Empty(player.CurrentGameRoomId);
            Assert.Equal(PlayerState.NotInGame, player.State);
        }

        [Theory]
        [InlineData("game1", "game2")]
        public void OnQuitGame_ShouldThrowPlayerNotInTheGameException(string currentGameId, string gameIdToQuit)
        {
            var player = new Player(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(), 
                currentGameId
                );

            PlayerNotInTheGameException expected = null;

            try
            {
                player.OnQuitGame(gameIdToQuit);
            }catch(PlayerNotInTheGameException e)
            {
                expected = e;
            }

            Assert.NotNull(expected);
            Assert.Equal(PlayerState.InGame, player.State);
            Assert.Equal(currentGameId, player.CurrentGameRoomId);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(0)]
        public void OnHitEvent_ShouldPass(int killPosibility)
        {
            var player = new Player(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(), 
                "fish_1"
                );

            var hitEvent = new HitEvent(
                hitBy: new HitBy(player),
                enemy: new Enemy(
                    id: new Guid().ToString(),
                    rewardMoney: new List<RewardMoney>()
                    {
                        new RewardMoney(
                            currency: Currency.CNY,
                            amount: 100,
                            precise: 100
                        )
                    }),
                bullet: new Bullet(
                    new Guid().ToString(),
                    new Cost(Currency.CNY, 1, 100)
                    )
                );

            if(killPosibility == 0)
            {
                var (isKill, dropCoinEvent) = player.OnHitEvent(hitEvent, killPosibility);

                Assert.False(isKill);
                Assert.Null(dropCoinEvent);
            }
            else
            {
                var (isKill, dropCoinEvent) = player.OnHitEvent(hitEvent, killPosibility);

                Assert.True(isKill);
                Assert.Equal(Currency.CNY, dropCoinEvent.Coin.Currency);
                Assert.Equal((uint)100, dropCoinEvent.Coin.Amount);
                Assert.Equal((uint)100, dropCoinEvent.Coin.Precise);
            }
         
        }
    }
}
