using AquaMan.Domain;
using System;
using System.Collections.Generic;

namespace AquaMan.DomainApi
{
    public class OrderService
    {
        private OrderRepository _repo;
        public OrderService(OrderRepository repo)
        {
            _repo = repo;
        }

        public Order CreateOrder(
            string agentId,
            string gameId,
            string accountId,
            uint amount,
            OrderType orderType
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

            if(accountId == null || accountId == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(accountId));
            }

            if(amount == 0)
            {
                throw new ArgumentInvalidException(nameof(amount));
            }


            var order = new Order(
                id: new Guid().ToString(),
                agentId: agentId,
                gameId: gameId,
                accountId: accountId,
                amount: amount,
                orderType: orderType,
                stateLogs: new List<OrderState>(),
                currentState: new OrderState(OrderStateType.PENDDING)
                );

            var result = _repo.Save(order);

            if (!result)
            {
                return null;
            }

            return order;
        }
    }
}
