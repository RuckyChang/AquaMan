using AquaMan.Domain;
using System.Collections.Generic;
using Xunit;

namespace AquaMan.DomainApi.Test
{
    public class AccountServiceTest
    {
        [Fact]
        public void CreateAccount_ShouldPass()
        {
            AccountRepository repo = new InMemoryAccountRepository();
            var accountService = new AccountService(repo);

            var account = accountService.CreateAccount(
                name: "test_1",
                password: "123456",
                agentId: "agent_1"
                );

            Assert.Equal("test_1", account.Name);
            Assert.Equal("123456", account.Password);
            Assert.Equal("agent_1", account.AgentId);
        }

        [Theory]
        [InlineData("", "12345", "agent1")]
        [InlineData("test", "", "agent1")]
        [InlineData("test", "12345", "")]
        public void CreateAccount_ShouldThrowInvalidData(
            string name,
            string password,
            string agentId
            )
        {
            AccountRepository repo = new InMemoryAccountRepository();
            var accountService = new AccountService(repo);

            ArgumentInvalidException expected = null;

            try
            {
                accountService.CreateAccount(
                    name: name,
                    password: password,
                    agentId: agentId
                    );
            }catch(ArgumentInvalidException e)
            {
                expected = e;
            }

            Assert.NotNull(expected);
        }
    }

    public class InMemoryAccountRepository : AccountRepository
    {
        private Dictionary<string, Account> _storage = new Dictionary<string, Account>();

        public List<Account> OfAgentId(string agentId)
        {
            List<Account> found = new List<Account>();
            foreach(var account in _storage.Values)
            {
                if(account.AgentId == agentId)
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
            foreach(var account in _storage.Values)
            {
                if(account.Name == name) {
                    found.Add(account);
                }
            }

            return found;
        }

        public bool Save(Account account)
        {
            if (_storage.ContainsKey(account.ID)){
                // maybe not all account info should update.
                _storage[account.ID] = account;
                return true;
            }

            _storage[account.ID] = account;
            return true;
        }
    }
}
