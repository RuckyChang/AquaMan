using AquaMan.Domain;
using AquaMan.DomainApi;
using System.Collections.Generic;

namespace AquaMan.WebsocketAdapter.Test
{
    public class InMemoryPlayerRepository : PlayerRepository
    {
        private Dictionary<string, Player> _storage = new Dictionary<string, Player>();

        public Player OfAccountId(string accountId)
        {
            foreach(var player in _storage.Values)
            {
                if(player.AccountId == accountId)
                {
                    return player;
                }
            }

            return null;
        }

        public List<Player> OfGameRoomId(string gameRoomId)
        {
            var found = new List<Player>();

            foreach(var player in _storage.Values)
            {
                if(player.CurrentGameRoomId == gameRoomId)
                {
                    found.Add(player);
                }
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
