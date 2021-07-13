using AquaMan.Domain;
using System.Collections.Generic;

namespace AquaMan.DomainApi
{
    public interface AccountRepository
    {
        public bool Save(Account account);
        public Account OfId(string id);
        public List<Account> OfAgentId(string agent);
        public List<Account> OfName(string name);

        public Account OfAgentIdAndName(string agentId, string name);

        public Account OfToken(string token);
    }
}
