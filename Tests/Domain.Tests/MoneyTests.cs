namespace Domain.Tests;

public class MoneyTests
{
	[Fact]
	public void Constructor_SetsAmountAndCurrency()
	{
		// Arrange & Act
		Money money = new(100.50m, "EUR");

		// Assert
		Assert.Equal(100.50m, money.Amount);
		Assert.Equal("EUR", money.Currency);
	}

	[Fact]
	public void Constructor_DefaultCurrencyIsUSD()
	{
		// Arrange & Act
		Money money = new(50.25m);

		// Assert
		Assert.Equal(50.25m, money.Amount);
		Assert.Equal("USD", money.Currency);
	}

	[Fact]
	public void Equality_SameAmountAndCurrency_AreEqual()
	{
		// Arrange
		Money money1 = new(75.00m, "GBP");
		Money money2 = new(75.00m, "GBP");

		// Act & Assert
		Assert.Equal(money1, money2);
	}

	[Fact]
	public void Equality_DifferentAmount_AreNotEqual()
	{
		// Arrange
		Money money1 = new(75.00m, "USD");
		Money money2 = new(75.01m, "USD");

		// Act & Assert
		Assert.NotEqual(money1, money2);
	}

	[Fact]
	public void Equality_DifferentCurrency_AreNotEqual()
	{
		// Arrange
		Money money1 = new(75.00m, "USD");
		Money money2 = new(75.00m, "EUR");

		// Act & Assert
		Assert.NotEqual(money1, money2);
	}
}