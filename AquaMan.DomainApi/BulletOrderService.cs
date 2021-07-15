using AquaMan.Domain;
using AquaMan.Domain.Entity;
using System;
using System.Collections.Generic;

namespace AquaMan.DomainApi
{
    public class BulletOrderService
    {
        private BulletOrderRepository _repo;

        public BulletOrderService(BulletOrderRepository repo)
        {
            _repo = repo;
        }

        public BulletOrder Create(
            string agentId,
            string gameId,
            string gameRoomId,
            string accountId,
            Cost cost
            )
        {
            if(agentId == null || agentId == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(agentId));
            }

            if(gameId == null || gameId == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(gameId));
            }

            if(gameRoomId == null || gameRoomId == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(gameRoomId));
            }

            if(accountId == null || accountId == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(accountId));
            }

            if(cost == null)
            {
                throw new ArgumentInvalidException(nameof(cost));
            }

            var currentState = new BulletOrderState(BulletOrderStateType.FIRED);
            var logs = new List<BulletOrderState>()
            {
                currentState
            };
            

            var bulletOrder = new BulletOrder(
                id: Guid.NewGuid().ToString(),
                agentId: agentId,
                gameId: gameId,
                gameRoomId: gameRoomId,
                accountId: accountId,
                currentState: currentState,
                stateLogs: logs,
                cost: cost
                );

            _repo.Save(bulletOrder);

            return bulletOrder;
        }

        public void Save(BulletOrder order)
        {
            _repo.Save(order);
        }
    }
}
