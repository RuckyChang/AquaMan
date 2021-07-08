using AquaMan.Domain;
using System;

namespace AquaMan.DomainApi
{
    public class AccountService
    {
        private AccountRepository _repo;
        public AccountService(AccountRepository accountRepository){
            _repo = accountRepository;
        }

        public Account CreateAccount(
            string name,
            string password,
            string agentId
            )
        {
            if(name == null || name == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(name));
            }

            if(password == null || password == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(password));
            }

            if (agentId == null || agentId == string.Empty)
            {
                throw new ArgumentInvalidException(nameof(agentId));
            }

            var id = new Guid().ToString();

            var wallet = new Wallet(
                currency: Domain.Entity.Currency.CNY,
                amount: 0,
                precise: 100
                );

            var account = new Account(
                id: id,
                name: name,
                password: password,
                agentId: agentId,
                token: "",
                lastLoginTime: DateTime.Now,
                wallet: wallet
                );

            var result =_repo.Save(account);

            if (!result)
            {
                return null;
            }

            return account;
        }
    }
}
