using AquaMan.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaMan.DomainApi
{
    public interface PlayerRepository
    {
        public bool Save(Player player);
        public Player OfId(string id);
        public Player OfAccountId(string accountId);
        public List<Player> OfGameId(string gameid);
    }
}
