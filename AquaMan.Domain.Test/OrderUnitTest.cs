using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AquaMan.Domain.Test
{
    public class OrderUnitTest
    {
        [Theory]
        [InlineData(OrderStateType.PENDDING, OrderStateType.LOCAL_COMMITTED)]
        [InlineData(OrderStateType.LOCAL_COMMITTED, OrderStateType.LOCAL_COMMITTED)]
        public void Commit_ShouldPass(OrderStateType currentStateType, OrderStateType expectedState)
        {
            var initState = new OrderState(OrderStateType.PENDDING);

            var stateLogs = new List<OrderState>()
            {
                initState
            };

            if(initState.StateType != currentStateType)
            {
                var currentState = new OrderState(currentStateType);

                stateLogs.Add(currentState);
            }

            var order = new Order(
                id: new Guid().ToString(),
                gameId: new Guid().ToString(),
                accountId: new Guid().ToString(),
                amount: 1,
                orderType: OrderType.DEPOSIT,
                stateLogs: stateLogs,
                currentState: new OrderState(currentStateType)
                );

            order.LocalCommit();

            Assert.Equal(expectedState, order.CurrentState.StateType);
            Assert.Equal(2, order.StateLogs.Count);
        }

        [Fact]
        public void Commit_ShouldThrowStateNotCorrectException()
        {
            var initState = new OrderState(OrderStateType.PENDDING);

            var stateLogs = new List<OrderState>()
            {
                initState
            };
          
         
            stateLogs.Add(new OrderState(OrderStateType.LOCAL_COMMITTED));
            stateLogs.Add(new OrderState(OrderStateType.REMOTE_COMMITTED));

            var order = new Order(
                id: new Guid().ToString(),
                gameId: new Guid().ToString(),
                accountId: new Guid().ToString(),
                amount: 1,
                orderType: OrderType.DEPOSIT,
                stateLogs: stateLogs,
                currentState: new OrderState(OrderStateType.REMOTE_COMMITTED)
                );

            StateNotCorrectException expected = null;

            try
            {

                order.LocalCommit();
            }catch(StateNotCorrectException e)
            {
                expected = e;
            }

            Assert.Equal(OrderStateType.REMOTE_COMMITTED, order.CurrentState.StateType);
            Assert.Equal(3, order.StateLogs.Count);
            Assert.NotNull(expected);
        }
    }
}
