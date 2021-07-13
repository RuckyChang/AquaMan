using AquaMan.Domain;
using AquaMan.DomainApi;
using System.Collections.Generic;

namespace AquaMan.WebsocketAdapter.Test
{
    public class InMemoryAccountRepository : AccountRepository
    {
        private Dictionary<string, Account> _storage = new Dictionary<string, Account>();

        public List<Account> OfAgentId(string agentId)
        {
            List<Account> found = new List<Account>();
            foreach (var account in _storage.Values)
            {
                if (account.AgentId == agentId)
                {
                    found.Add(account);
                }
            }

            return found;
        }

        public Account OfId(string id)
        {
            return _storage[id];
        }

        public List<Account> OfName(string name)
        {
            List<Account> found = new List<Account>();
            foreach (var account in _storage.Values)
            {
                if (account.Name == name)
                {
                    found.Add(account);
                }
            }

            return found;
        }

        public Account OfAgentIdAndName(string agentId, string name)
        {
            List<Account> found = new List<Account>();
            foreach (var account in _storage.Values)
            {
                if (account.Name == name && account.AgentId == agentId)
                {
                    found.Add(account);
                }
            }

            if(found.Count == 0)
            {
                return null;
            }

            return found[0];
        }

        public Account OfToken(string token)
        {
            List<Account> found = new List<Account>();
            foreach (var account in _storage.Values)
            {
                if (account.Token == token)
                {
                    found.Add(account);
                }
            }

            if(found.Count == 0)
            {
                return null;
            }

            return found[0];
        }

        public bool Save(Account account)
        {
            if (_storage.ContainsKey(account.ID))
            {
                // maybe not all account info should update.
                _storage[account.ID] = account;
                return true;
            }

            _storage[account.ID] = account;
            return true;
        }
    }
}
