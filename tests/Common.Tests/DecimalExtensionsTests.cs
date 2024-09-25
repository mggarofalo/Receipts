namespace Common.Tests;

public class DecimalExtensionsTests
{
	[Fact]
	public void Decimal_Between_ReturnsTrue_WhenValueIsWithinRange()
	{
		Assert.True(5m.Between(1m, 10m));
	}

	[Fact]
	public void Decimal_Between_ReturnsFalse_WhenValueIsAboveRange()
	{
		Assert.False(15m.Between(1m, 10m));
	}

	[Fact]
	public void Decimal_Between_ReturnsFalse_WhenValueIsBelowRange()
	{
		Assert.False((-5m).Between(1m, 10m));
	}

	[Fact]
	public void Decimal_Between_ReturnsTrue_WhenValueEqualsMin()
	{
		Assert.True(1m.Between(1m, 10m));
	}

	[Fact]
	public void Decimal_Between_ReturnsTrue_WhenValueEqualsMax()
	{
		Assert.True(10m.Between(1m, 10m));
	}
}