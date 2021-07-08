using AquaMan.Domain;
using System.Collections.Generic;
using Xunit;

namespace AquaMan.DomainApi.Test
{
    public class PlayerServiceTest
    {
        [Fact]
        public void CreatePlayer_ShouldPass()
        {
            var playerRepository = new InMemoryPlayerRepository();

            var playerService = new PlayerService(playerRepository);

            var player = playerService.CreatePlayer(
                accountId: "accountId_1",
                gameId: "gameId_1"
                );

            Assert.Equal("accountId_1", player.AccountId);
            Assert.Equal("gameId_1", player.CurrentGameId);
        }
    }

    class InMemoryPlayerRepository : PlayerRepository
    {
        private Dictionary<string, Player> _storage = new Dictionary<string, Player>();
        public Player OfAccountId(string accountId)
        {
            Player found = null;

            foreach(var player in _storage.Values)
            {
                if (player.AccountId == accountId)
                {
                    found = player;
                    break;
                }
            }

            return found;
        }

        public List<Player> OfGameId(string gameid)
        {
            List<Player> found = new List<Player>();

            foreach(var player in _storage.Values)
            {
                found.Add(player);
            }

            return found;
        }

        public Player OfId(string id)
        {
            return _storage[id];
        }

        public bool Save(Player player)
        {
            if (_storage.ContainsKey(player.ID))
            {
                _storage[player.ID] = player;
                return true;
            }

            _storage[player.ID] = player;
            return true;
        }
    }
}
