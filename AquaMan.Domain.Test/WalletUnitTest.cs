using AquaMan.Domain.Entity;
using Xunit;

namespace AquaMan.Domain.Test
{
    public class WalletUnitTest
    {
        [Theory]
        [InlineData(Currency.USD)]
        [InlineData(Currency.TWD)]
        [InlineData(Currency.CNY)]
        public void IsSameCurrency_ShouldPass(Currency currency)
        {
            Wallet wallet = new Wallet(
                currency: currency,
                amount: 10000,
                precise: 100
                );

            Assert.True(wallet.IsSameCurrency(new Cost(currency: currency, 10000, 100)));
        }

        [Theory]
        [InlineData(Currency.TWD)]
        [InlineData(Currency.CNY)]
        public void IsSameCurrency_ShouldNotPass(Currency currency)
        {
            Wallet wallet = new Wallet(
                currency: Currency.USD,
                amount: 10000,
                precise: 100
                );

            Assert.False(wallet.IsSameCurrency(new Cost(currency: currency, 10000, 100)));
        }

        [Theory]
        [InlineData(100, 100, 100)]
        [InlineData(100, 1, 100)]
        public void IsEnough_ShouldPass(uint amountInWallet, uint amountToCheck, uint precise)
        {
            Wallet wallet = new Wallet(
                currency: Currency.USD,
                amount: amountInWallet,
                precise: precise
                );

            Assert.True(wallet.IsEnough(new Cost(currency: Currency.USD, amount: amountToCheck, precise: precise)));
        }

        [Theory]
        [InlineData(1, 2, 100)]
        [InlineData(10, 100, 100)]
        public void IsEnough_ShouldNotPass(uint amountInWallet, uint amountToCheck, uint precise)
        {
            Wallet wallet = new Wallet(
                currency: Currency.USD,
                amount: amountInWallet,
                precise: precise
                );

            Assert.False(wallet.IsEnough(new Cost(currency: Currency.USD, amount: amountToCheck, precise: precise)));
        }

        [Theory]
        [InlineData(100, 1, 100,101)]
        [InlineData(100, 2, 100, 102)]
        public void Increase_ShouldPass(uint amountInWallet, uint amountToIncrease, uint precise, uint expected)
        {
            Wallet wallet = new Wallet(
                currency: Currency.USD,
                amount: amountInWallet,
                precise: precise
                );

            wallet.Increase(new Cost(
                currency: Currency.USD,
                amount: amountToIncrease,
                precise: precise
                ));
            Assert.Equal(wallet.Amount, expected);
        }

        [Theory]
        [InlineData(100, 1, 100, 99)]
        [InlineData(100, 3, 100, 97)]
        public void Decrease_ShouldPass(uint amountInWallet, uint amountToDecrease, uint precise, uint expected)
        {
            Wallet wallet = new Wallet(
                currency: Currency.USD,
                amount: amountInWallet,
                precise: precise
                );

            var result = wallet.Decrease(new Cost(currency: Currency.USD, amount: amountToDecrease, precise: precise));

            Assert.True(result);
            Assert.Equal(wallet.Amount, expected);
        }

        [Theory]
        [InlineData(100, 102, 100)]
        [InlineData(100, 103, 100)]
        public void Decrease_ShouldNotPass(uint amountInWallet, uint amountToDecrease, uint precise)
        {
            Wallet wallet = new Wallet(
                currency: Currency.USD,
                amount: amountInWallet,
                precise: precise
                );

            var result = wallet.Decrease(new Cost(currency: Currency.USD, amount: amountToDecrease, precise));
            
            Assert.False(result);
            Assert.Equal(wallet.Amount, amountInWallet);
        }
    }
}
