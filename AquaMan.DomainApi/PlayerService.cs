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

        public Player CreatePlayer(string accountId, string gameId)
        {
            if(accountId == null || accountId == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(accountId));
            }

            if(gameId ==null)
            {
                throw new ArgumentInvalidException(nameof(gameId));
            }

            var player = new Player(
                id: new Guid().ToString(),
                accountId: accountId,
                gameId: gameId
                );

            if (_repo.Save(player))
            {
                return player;
            }

            return null;
        }
    }
}
