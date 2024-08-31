using Common;

namespace Domain.Tests;

public class MoneyTests
{
	[Fact]
	public void Money_CreatedWithAmount_ShouldHaveCorrectAmount()
	{
		var money = new Money(100.50m);
		Assert.Equal(100.50m, money.Amount);
	}

	[Fact]
	public void Money_CreatedWithoutCurrency_ShouldDefaultToUSD()
	{
		var money = new Money(50);
		Assert.Equal(Currency.USD, money.Currency);
	}

	[Fact]
	public void Money_EqualityCheck_ShouldBeTrue_WhenAmountsAreEqual()
	{
		var money1 = new Money(75.25m);
		var money2 = new Money(75.25m);
		Assert.Equal(money1, money2);
	}

	[Fact]
	public void Money_EqualityCheck_ShouldBeFalse_WhenAmountsAreDifferent()
	{
		var money1 = new Money(75.25m);
		var money2 = new Money(75.26m);
		Assert.NotEqual(money1, money2);
	}

	[Fact]
	public void Money_ToString_ShouldReturnCorrectString()
	{
		var money = new Money(123.45m);
		Assert.Equal("Money { Amount = 123.45, Currency = USD }", money.ToString());
	}
}