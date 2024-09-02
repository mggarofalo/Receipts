using Common;

namespace Domain.Tests;

public class MoneyTests
{
	[Fact]
	public void Money_CreatedWithAmount_ShouldHaveCorrectAmount()
	{
		Money money = new(100.50m);
		Assert.Equal(100.50m, money.Amount);
	}

	[Fact]
	public void Money_CreatedWithoutCurrency_ShouldDefaultToUSD()
	{
		Money money = new(50);
		Assert.Equal(Currency.USD, money.Currency);
	}

	[Fact]
	public void Money_EqualityCheck_ShouldBeTrue_WhenAmountsAreEqual()
	{
		Money money1 = new(75.25m);
		Money money2 = new(75.25m);
		Assert.Equal(money1, money2);
	}

	[Fact]
	public void Money_EqualityCheck_ShouldBeFalse_WhenAmountsAreDifferent()
	{
		Money money1 = new(75.25m);
		Money money2 = new(75.26m);
		Assert.NotEqual(money1, money2);
	}

	[Fact]
	public void Money_ToString_ShouldReturnCorrectString()
	{
		Money money = new(123.45m);
		Assert.Equal("Money { Amount = 123.45, Currency = USD }", money.ToString());
	}

	[Fact]
	public void Money_Zero_ShouldReturnCorrectResult()
	{
		Money money = Money.Zero;
		Assert.Equal(0m, money.Amount);
	}

	[Fact]
	public void Money_Addition_ShouldReturnCorrectResult()
	{
		Money money1 = new(100.50m);
		Money money2 = new(200.75m);
		Money result = money1 + money2;
		Assert.Equal(new Money(301.25m), result);
	}

	[Fact]
	public void Money_Subtraction_ShouldReturnCorrectResult()
	{
		Money money1 = new(200.75m);
		Money money2 = new(100.50m);
		Money result = money1 - money2;
		Assert.Equal(new Money(100.25m), result);
	}

	[Fact]
	public void Money_Multiplication_ShouldReturnCorrectResult()
	{
		Money money1 = new(100.50m);
		Money money2 = new(2);
		Money result = money1 * money2;
		Assert.Equal(new Money(201.00m), result);
	}

	[Fact]
	public void Money_Division_ShouldReturnCorrectResult()
	{
		Money money1 = new(201.00m);
		Money money2 = new(2);
		Money result = money1 / money2;
		Assert.Equal(new Money(100.50m), result);
	}
}