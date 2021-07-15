using AquaMan.Domain;
using AquaMan.DomainApi;
using System.Collections.Generic;

namespace AquaMan.WebsocketAdapter.Test
{
    public class InMemoryBulletOrderRepository : BulletOrderRepository
    {
        private Dictionary<string, BulletOrder> _storage = new Dictionary<string, BulletOrder>();
        public List<BulletOrder> OfAccountId(string accountId)
        {
            List<BulletOrder> found = new List<BulletOrder>();
            foreach (var bulletOrder in _storage.Values)
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
