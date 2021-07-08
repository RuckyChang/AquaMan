using AquaMan.Domain;
using System.Collections.Generic;
using Xunit;

namespace AquaMan.DomainApi.Test
{
    public class OrderServiceTest
    {
        [Fact]
        public void CreateOrder_ShouldPass()
        {
            OrderRepository orderRepository = new InMemoryOrderRepository();
            var orderService = new OrderService(orderRepository);

            var order = orderService.CreateOrder(
                agentId: "agentId_1",
                gameId: "gameId_2",
                accountId: "accountId_1",
                amount: 1,
                orderType: OrderType.DEPOSIT
                );

            Assert.Equal("agentId_1", order.AgentId);
            Assert.Equal("gameId_2", order.GameId);
            Assert.Equal("accountId_1", order.AccountID);
            Assert.Equal((uint)1, order.Amount);
            Assert.Equal(OrderType.DEPOSIT, order.OrderType);
            Assert.Equal(OrderStateType.PENDDING, order.CurrentState.StateType);
        }

        [Theory]
        [InlineData("", "gameId_2", "account_id_1", 1)]
        [InlineData("agentId_1", "", "account_id_1", 1)]
        [InlineData("agentId_1", "gameId_2", "", 1)]
        [InlineData("agentId_1", "gameId_2", "account_id_1", 0)]
        public void CreateOrder_ShouldThrowArgumentInvalid(
            string agentId, 
            string gameId, 
            string accountId, 
            uint amount
            )
        {
            OrderRepository orderRepository = new InMemoryOrderRepository();

            var orderService = new OrderService(orderRepository);

            ArgumentInvalidException expected = null;

            try
            {
                var order = orderService.CreateOrder(
                  agentId: agentId,
                  gameId: gameId,
                  accountId: accountId,
                  amount: amount,
                  orderType: OrderType.DEPOSIT
                  );
            }
            catch(ArgumentInvalidException e)
            {
                expected = e;
            }

            Assert.NotNull(expected);
        }
    }

    class InMemoryOrderRepository : OrderRepository
    {
        private Dictionary<string, Order> _storage = new Dictionary<string, Order>();
        public List<Order> OfAccountId(string accountId)
        {
            List<Order> found = new List<Order>();

            foreach(var order in _storage.Values)
            {
                if(order.AccountID == accountId)
                {
                    found.Add(order);
                }
            }

            return found;
        }

        public List<Order> OfAgentId(string agentId)
        {
            List<Order> found = new List<Order>();

            foreach(var order in _storage.Values)
            {
                if(order.AgentId == agentId)
                {
                    found.Add(order);
                }
            }

            return found;
        }

        public List<Order> OfGameId(string gameId)
        {
            List<Order> found = new List<Order>();

            foreach(var order in _storage.Values)
            {
                if(order.GameId == gameId)
                {
                    found.Add(order);
                }
            }

            return found;
        }

        public Order OfId(string id)
        {
            return _storage[id];
        }

        public List<Order> OfOrderType(OrderType orderType)
        {
            List<Order> found = new List<Order>();

            foreach(var order in _storage.Values)
            {
                if(order.OrderType == orderType)
                {
                    found.Add(order);
                }
            }

            return found;
        }

        public List<Order> OfState(OrderStateType orderStateType)
        {
            List<Order> found = new List<Order>();

            foreach(var order in _storage.Values)
            {
                if(order.CurrentState.StateType == orderStateType)
                {
                    found.Add(order);
                }
            }

            return found;
        }

        public bool Save(Order order)
        {
            if (_storage.ContainsKey(order.ID))
            {
                _storage[order.ID] = order;
                return true;
            }

            _storage[order.ID] = order;

            return true;
        }
    }
}
