using AquaMan.Domain;
using AquaMan.Domain.Entity;
using System;
using System.Collections.Generic;
using Xunit;

namespace AquaMan.DomainApi.Test
{
    public class BulletServiceTest
    {
        [Fact]
        public void CreateBulletOrder_ShouldPass()
        {
            var bulletOrderRepository = new InMemoryBulletOrderRepository();
            var bulletOrderService = new BulletOrderService(bulletOrderRepository);

            var bulletOrder = bulletOrderService.Create(
                agentId: Guid.NewGuid().ToString(),
                gameId: Guid.NewGuid().ToString(),
                gameRoomId: Guid.NewGuid().ToString(),
                accountId: Guid.NewGuid().ToString(),
                cost: new Cost(
                    currency: Currency.CNY,
                    amount: 100,
                    precise: 100
                    )
                );

            Assert.Equal(BulletOrderStateType.FIRED, bulletOrder.CurrentState.StateType);
            Assert.Single(bulletOrder.StateLogs);
        }

        class InMemoryBulletOrderRepository : BulletOrderRepository
        {
            private Dictionary<string, BulletOrder> _storage = new Dictionary<string, BulletOrder>();
            public List<BulletOrder> OfAccountId(string accountId)
            {
                List<BulletOrder> found = new List<BulletOrder>();
                foreach(var bulletOrder in _storage.Values)
                {
                    if (bulletOrder.AccountID == accountId)
                    {
                        found.Add(bulletOrder);
                    }
                }

                return found;
            }

            public BulletOrder OfId(string ID)
            {
                return _storage[ID];
            }

            public bool Save(BulletOrder bulletOrder)
            {
                if (_storage.ContainsKey(bulletOrder.ID))
                {
                    _storage[bulletOrder.ID] = bulletOrder;
                    return true;
                }

                _storage[bulletOrder.ID] = bulletOrder;
                return true;
            }
        }
    }
}
