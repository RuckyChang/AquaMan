using AquaMan.Domain.Entity;
using System;
using System.Collections.Generic;
using Xunit;

namespace AquaMan.Domain.Test
{
    public class BulletOrderUnitTest
    {

        [Fact]
        public void OnHit_ShouldPass()
        {

            var currentState = new BulletOrderState(BulletOrderStateType.FIRED);
            List<BulletOrderState> logs = new List<BulletOrderState>();

            logs.Add(currentState);

            var order = new BulletOrder(
                id: Guid.NewGuid().ToString(),
                agentId: Guid.NewGuid().ToString(),
                gameId: Guid.NewGuid().ToString(),
                gameRoomId: Guid.NewGuid().ToString(),
                accountId: Guid.NewGuid().ToString(),
                currentState: currentState,
                stateLogs : logs,
                cost: new Cost(
                    currency: Currency.CNY,
                    amount: 100,
                    precise: 100
                    )
                );

            order.OnHit(new HitTarget(Guid.NewGuid().ToString()));

            Assert.Equal(BulletOrderStateType.HIT, order.CurrentState.StateType);
            Assert.Equal(2, order.StateLogs.Count);
        }
    }
}
