using AquaMan.Domain;
using System;

namespace AquaMan.DomainApi
{
    public class PlayerService
    {
        private PlayerRepository _repo;

        public PlayerService(PlayerRepository repo)
        {
            _repo = repo;
        }

        public Player CreatePlayer(string accountId, string gameRoomId)
        {

            if(accountId == null || accountId == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(accountId));
            }

            if(gameRoomId ==null)
            {
                throw new ArgumentInvalidException(nameof(gameRoomId));
            }

            var player = new Player(
                  id: new Guid().ToString(),
                  accountId: accountId,
                  gameRoomId: gameRoomId
            );

            if (_repo.Save(player))
            {
                return player;
            }

            return null;
        }

        public Player OfAccountId(string accountId)
        {
            if(accountId == null || accountId == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(accountId));
            }

            var player = _repo.OfAccountId(accountId);

            if (player == null)
            {
                player = new Player(
                      id: Guid.NewGuid().ToString(),
                      accountId: accountId
                );
            }

            return player;
        }

        public void Save(Player player)
        {
            _repo.Save(player);
        }
    }
}
