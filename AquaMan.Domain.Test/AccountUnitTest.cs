using AquaMan.Domain.Entity;
using AquaMan.Domain.GameEvent;
using System;
using Xunit;

namespace AquaMan.Domain.Test
{
    public class AccountUnitTest
    {
        [Theory]
        [InlineData("foo","bar")]
        public void Login_ShouldPass(string name, string password)
        {
            Account account = new Account(
                id: new Guid().ToString(),
                name: "foo",
                password: "bar",
                agent: "123456",
                token: "",
                lastLoginTime: null,
                wallet: new Wallet(currency: Currency.USD, amount: 0, precise: 1)
                );

            account.Login(name, password);

            Assert.NotNull(account.LastLoginTime);
            Assert.NotEmpty(account.Token);
        }

        [Theory]
        [InlineData("foo", "foor")]
        public void Login_ShouldThrowException(string name, string password)
        {
            Account account = new Account(
                id: new Guid().ToString(),
                name: "foor",
                password: "bar",
                agent: "123456",
                token: "",
                lastLoginTime: null,
                wallet: new Wallet(currency: Currency.USD, amount: 0, precise: 1)
            );

            LoginFailedException expectedException = null;
            try
            {
                account.Login(name, password);
            }catch(LoginFailedException loginFailedException)
            {
                expectedException = loginFailedException;
            }

            Assert.NotNull(expectedException);
            Assert.Null(account.LastLoginTime);
            Assert.Empty(account.Token);
        }

        [Theory]
        [InlineData(Currency.USD, 100, 1)]
        [InlineData(Currency.USD, 102, 1)]
        public void Deposit_ShouldPass(Currency currency, uint amount, uint precise)
        {
            Account account = new Account(
                id: new Guid().ToString(),
                name: "foor",
                password: "bar",
                agent: "123456",
                token: "",
                lastLoginTime: null,
                wallet: new Wallet(currency: Currency.USD, amount: 0, precise: 1)
            );

            account.Deposit(new Cost(currency: currency, amount: amount, precise: precise));

            Assert.Equal(amount, account.Wallet.Amount);
        }

        [Theory]
        [InlineData(Currency.USD, 100, 1)]
        public void Deposit_ShouldThrowNotMatchExcpetion(Currency currency, uint amount, uint precise)
        {
            Account account = new Account(
                id: new Guid().ToString(),
                name: "foor",
                password: "bar",
                agent: "123456",
                token: "",
                lastLoginTime: null,
                wallet: new Wallet(currency: Currency.TWD, amount: 0, precise: 1)
            );

            CurrencyNotMatchException expectedException = null;

            try
            {
                account.Deposit(new Cost(currency: currency, amount: amount, precise: precise));
            }catch(CurrencyNotMatchException e)
            {
                expectedException = e;
            }

            Assert.NotNull(expectedException);
            Assert.Equal((ulong)0, account.Wallet.Amount);
        }

        [Theory]
        [InlineData(100,1, 99)]
        [InlineData(100, 100, 0)]
        public void Withdraw_ShouldPass(uint amountInWallet, uint amountToWithdraw, uint expected)
        {
            Account account = new Account(
                id: new Guid().ToString(),
                name: "foor",
                password: "bar",
                agent: "123456",
                token: "",
                lastLoginTime: null,
                wallet: new Wallet(currency: Currency.TWD, amount: amountInWallet, precise: 1)
            );

            account.Withdraw(new Cost(currency: Currency.TWD, amount: amountToWithdraw, precise: 1));

            Assert.Equal(expected, account.Wallet.Amount);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(10, 100)]
        public void Withdraw_ShouldThrowNotEnoughException(uint amountInWallet, uint amountToWithdraw)
        {
            Account account = new Account(
                id: new Guid().ToString(),
                name: "foor",
                password: "bar",
                agent: "123456",
                token: "",
                lastLoginTime: null,
                wallet: new Wallet(currency: Currency.TWD, amount: amountInWallet, precise: 1)
            );

            NotEnoughAmountException expectedException = null;

            try
            {
                account.Withdraw(new Cost(
                    currency: Currency.TWD,
                    amount: amountToWithdraw,
                    precise: 1
                    ));
            }catch(NotEnoughAmountException e)
            {
                expectedException = e;
            }

            Assert.NotNull(expectedException);
            Assert.Equal(amountInWallet, account.Wallet.Amount);
        }

        [Theory]
        [InlineData(Currency.TWD,Currency.USD, 1)]
        public void Withdraw_ShouldThrowNotMatchException(Currency walletCurrency, Currency currency, uint amountToWithdraw)
        {
            Account account = new Account(
               id: new Guid().ToString(),
               name: "foor",
               password: "bar",
               agent: "123456",
               token: "",
               lastLoginTime: null,
               wallet: new Wallet(currency: walletCurrency, amount: 1, precise: 1)
           );

            CurrencyNotMatchException expected = null;
            try
            {
                account.Withdraw(new Cost(currency: currency, amount: amountToWithdraw, precise: 1));
            }catch(CurrencyNotMatchException e)
            {
                expected = e;
            }

            Assert.NotNull(expected);
            Assert.Equal((ulong)1, account.Wallet.Amount);
        }

        [Theory]
        [InlineData(100, 10, 90)]
        [InlineData(100, 11, 89)]
        public void OnShootEvent_ShouldPass(uint amountInWallet, uint amountOfBullet, uint expectedAmount)
        {
            Account account = new Account(
               id: new Guid().ToString(),
               name: "foor",
               password: "bar",
               agent: "123456",
               token: "",
               lastLoginTime: null,
               wallet: new Wallet(currency: Currency.TWD, amount: amountInWallet, precise: 1)
           );

            Player player = new Player(
                accountId: account.ID,
                state: State.NORMAL,
                gameId: "fish_1"
                );

            var shootEvent = new ShootEvent(
                shootBy: new ShootBy(
                    account: account,
                    player: player
                    ),
                bulletName: "silver_bullet",
                new Cost(
                    currency: Currency.TWD,
                    amount: amountOfBullet,
                    precise: 1
                    )
                );

            account.OnShootEvent(shootEvent);

            Assert.Equal((ulong)expectedAmount, account.Wallet.Amount);
        }

        [Theory]
        [InlineData(100, 199)]
        [InlineData(100, 101)]
        public void OnShootEvent_ShouldThrowNotEnoughException(uint amountInWallet, uint bulletAmount)
        {
            Account account = new Account(
               id: new Guid().ToString(),
               name: "foor",
               password: "bar",
               agent: "123456",
               token: "",
               lastLoginTime: null,
               wallet: new Wallet(currency: Currency.TWD, amount: amountInWallet, precise: 1)
            );

            Player player = new Player(
                accountId: account.ID,
                state: State.NORMAL,
                gameId: "fish_1"
            );

            var shootEvent = new ShootEvent(
                shootBy: new ShootBy(
                    account: account,
                    player: player
                    ),
                bulletName: "silver_bullet",
                new Cost(
                    currency: Currency.TWD,
                    amount: bulletAmount,
                    precise: 1
                    )
            );

            NotEnoughAmountException expected = null;

            try
            {
                account.OnShootEvent(shootEvent);
            }catch(NotEnoughAmountException e)
            {
                expected = e;
            }

            Assert.NotNull(expected);
            Assert.Equal(amountInWallet, account.Wallet.Amount);
        }

        [Theory]
        [InlineData(100, 199)]
        public void OnShootEvent_ShouldThrowNotMatchException(uint amountInWallet, uint bulletAmount)
        {
            Account account = new Account(
              id: new Guid().ToString(),
              name: "foor",
              password: "bar",
              agent: "123456",
              token: "",
              lastLoginTime: null,
              wallet: new Wallet(currency: Currency.USD, amount: amountInWallet, precise: 1)
           );

            Player player = new Player(
                accountId: account.ID,
                state: State.NORMAL,
                gameId: "fish_1"
            );

            var shootEvent = new ShootEvent(
                shootBy: new ShootBy(
                    account: account,
                    player: player
                    ),
                bulletName: "silver_bullet",
                new Cost(
                    currency: Currency.TWD,
                    amount: bulletAmount,
                    precise: 1
                    )
            );

            CurrencyNotMatchException expected = null;

            try
            {
                account.OnShootEvent(shootEvent);
            }
            catch (CurrencyNotMatchException e)
            {
                expected = e;
            }

            Assert.NotNull(expected);
            Assert.Equal(amountInWallet, account.Wallet.Amount);
        }

        [Theory]
        [InlineData(100, 100, 200)]
        public void OnCoinDrop_ShouldPass(uint amountInWallet, uint dropAmount, uint expected)
        {
            Account account = new Account(
              id: new Guid().ToString(),
              name: "foor",
              password: "bar",
              agent: "123456",
              token: "",
              lastLoginTime: null,
              wallet: new Wallet(currency: Currency.USD, amount: amountInWallet, precise: 1)
           );

            var dropCoinEvent = new DropCoinEvent(coin: new Cost(
                currency: Currency.USD,
                amount: dropAmount,
                precise: 1
                ));

            account.OnCoinDrop(dropCoinEvent);

            Assert.Equal(expected, account.Wallet.Amount);
        }

        [Theory]
        [InlineData(Currency.TWD)]
        public void OnCoinDrop_ShouldThrowNotMatchException(Currency currency)
        {
            Account account = new Account(
              id: new Guid().ToString(),
              name: "foor",
              password: "bar",
              agent: "123456",
              token: "",
              lastLoginTime: null,
              wallet: new Wallet(currency: Currency.USD, amount: 100, precise: 1)
            );

            var dropCoinEvent = new DropCoinEvent(coin: new Cost(
             currency: currency,
             amount: 1,
             precise: 1
             ));

            CurrencyNotMatchException expected = null;

            try
            {
                account.OnCoinDrop(dropCoinEvent);

            }catch(CurrencyNotMatchException e)
            {
                expected = e;
            }

            Assert.NotNull(expected);
            Assert.Equal((ulong)100, account.Wallet.Amount);
        }
    }
}
