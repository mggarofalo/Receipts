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

	[Fact]
	public void Money_Zero_ShouldReturnCorrectResult()
	{
		var money = Money.Zero;
		Assert.Equal(0m, money.Amount);
	}

	[Fact]
	public void Money_Addition_ShouldReturnCorrectResult()
	{
		var money1 = new Money(100.50m);
		var money2 = new Money(200.75m);
		var result = money1 + money2;
		Assert.Equal(new Money(301.25m), result);
	}

	[Fact]
	public void Money_Subtraction_ShouldReturnCorrectResult()
	{
		var money1 = new Money(200.75m);
		var money2 = new Money(100.50m);
		var result = money1 - money2;
		Assert.Equal(new Money(100.25m), result);
	}

	[Fact]
	public void Money_Multiplication_ShouldReturnCorrectResult()
	{
		var money1 = new Money(100.50m);
		var money2 = new Money(2);
		var result = money1 * money2;
		Assert.Equal(new Money(201.00m), result);
	}

	[Fact]
	public void Money_Division_ShouldReturnCorrectResult()
	{
		var money1 = new Money(201.00m);
		var money2 = new Money(2);
		var result = money1 / money2;
		Assert.Equal(new Money(100.50m), result);
	}
}