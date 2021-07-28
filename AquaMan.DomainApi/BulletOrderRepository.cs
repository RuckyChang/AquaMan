using AquaMan.Domain;
using System.Collections.Generic;

namespace AquaMan.DomainApi
{
    public interface BulletOrderRepository
    {
        public BulletOrder OfId(string ID);
        public List<BulletOrder> OfAccountId(string accountId);
        public List<BulletOrder> OfAccountId(string accountId, BulletOrderStateType state);
        public List<BulletOrder> OfAccountId(string accountId, BulletOrderStateType state, int count);
        public bool Save(BulletOrder bulletOrder);

    }
}
