using AquaMan.Domain;
using System.Collections.Generic;

namespace AquaMan.DomainApi
{
    public interface OrderRepository
    {
        public bool Save(Order order);
        public Order OfId(string id);
        public List<Order> OfAgentId(string agentId);
        public List<Order> OfGameId(string gameId);
        public List<Order> OfAccountId(string accountId);
        public List<Order> OfOrderType(OrderType orderType);
        public List<Order> OfState(OrderStateType orderStateType);
    }
}
