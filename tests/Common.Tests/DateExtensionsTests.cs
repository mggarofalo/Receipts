namespace Common.Tests;

public class DateExtensionsTests
{
	[Fact]
	public void DateTime_Between_ReturnsTrue_WhenDateIsWithinRange()
	{
		// Arrange
		DateTime min = new(2023, 1, 1);
		DateTime max = new(2023, 12, 31);
		DateTime value = new(2023, 6, 15);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void DateTime_Between_ReturnsFalse_WhenDateIsBeforeRange()
	{
		// Arrange
		DateTime min = new(2023, 1, 1);
		DateTime max = new(2023, 12, 31);
		DateTime value = new(2022, 12, 31);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void DateTime_Between_ReturnsFalse_WhenDateIsAfterRange()
	{
		// Arrange
		DateTime min = new(2023, 1, 1);
		DateTime max = new(2023, 12, 31);
		DateTime value = new(2024, 1, 1);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void DateTime_Between_ReturnsTrue_WhenDateIsEqualToMin()
	{
		// Arrange
		DateTime min = new(2023, 1, 1);
		DateTime max = new(2023, 12, 31);
		DateTime value = new(2023, 1, 1);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void DateTime_Between_ReturnsTrue_WhenDateIsEqualToMax()
	{
		// Arrange
		DateTime min = new(2023, 1, 1);
		DateTime max = new(2023, 12, 31);
		DateTime value = new(2023, 12, 31);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void DateOnly_Between_ReturnsTrue_WhenDateIsWithinRange()
	{
		// Arrange
		DateOnly min = new(2023, 1, 1);
		DateOnly max = new(2023, 12, 31);
		DateOnly value = new(2023, 6, 15);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void DateOnly_Between_ReturnsFalse_WhenDateIsBeforeRange()
	{
		// Arrange
		DateOnly min = new(2023, 1, 1);
		DateOnly max = new(2023, 12, 31);
		DateOnly value = new(2022, 12, 31);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void DateOnly_Between_ReturnsFalse_WhenDateIsAfterRange()
	{
		// Arrange
		DateOnly min = new(2023, 1, 1);
		DateOnly max = new(2023, 12, 31);
		DateOnly value = new(2024, 1, 1);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void DateOnly_Between_ReturnsTrue_WhenDateIsEqualToMin()
	{
		// Arrange
		DateOnly min = new(2023, 1, 1);
		DateOnly max = new(2023, 12, 31);
		DateOnly value = new(2023, 1, 1);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void DateOnly_Between_ReturnsTrue_WhenDateIsEqualToMax()
	{
		// Arrange
		DateOnly min = new(2023, 1, 1);
		DateOnly max = new(2023, 12, 31);
		DateOnly value = new(2023, 12, 31);

		// Act
		bool result = value.Between(min, max);

		// Assert
		Assert.True(result);
	}
}