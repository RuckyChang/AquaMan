using AquaMan.Domain;
using System.Collections.Generic;

namespace AquaMan.DomainApi
{
    public interface PlayerRepository
    {
        public bool Save(Player player);
        public Player OfId(string id);
        public Player OfAccountId(string accountId);
        public List<Player> OfGameRoomId(string gameRoomId);
    }
}
