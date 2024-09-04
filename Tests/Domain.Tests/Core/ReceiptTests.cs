using Domain.Core;
using SampleData.Domain.Core;

namespace Domain.Tests.Core;

public class ReceiptTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesReceipt()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string location = "Test Store";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		Money taxAmount = new(5.00m);
		string description = "Test Receipt";

		// Act
		Receipt receipt = new(id, location, date, taxAmount, description);

		// Assert
		Assert.Equal(id, receipt.Id);
		Assert.Equal(location, receipt.Location);
		Assert.Equal(date, receipt.Date);
		Assert.Equal(taxAmount, receipt.TaxAmount);
		Assert.Equal(description, receipt.Description);
	}

	[Fact]
	public void Constructor_NullId_CreatesReceiptWithNullId()
	{
		// Arrange
		string location = "Test Store";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		Money taxAmount = new(5.00m);

		// Act
		Receipt receipt = new(null, location, date, taxAmount);

		// Assert
		Assert.Null(receipt.Id);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Constructor_InvalidLocation_ThrowsArgumentException(string invalidLocation)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		Money taxAmount = new(5.00m);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Receipt(id, invalidLocation, date, taxAmount));
		Assert.StartsWith(Receipt.LocationCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Constructor_FutureDate_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string location = "Test Store";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
		Money taxAmount = new(5.00m);

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Receipt(id, location, date, taxAmount));
		Assert.StartsWith(Receipt.DateCannotBeInTheFuture, exception.Message);
	}

	[Fact]
	public void Equals_SameReceipt_ReturnsTrue()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string location = "Test Store";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		Money taxAmount = new(5.00m);

		Receipt receipt1 = new(id, location, date, taxAmount);
		Receipt receipt2 = new(id, location, date, taxAmount);

		// Act & Assert
		Assert.True(receipt1 == receipt2);
		Assert.False(receipt1 != receipt2);
		Assert.True(receipt1.Equals(receipt2));
	}

	[Fact]
	public void Equals_DifferentReceipt_ReturnsFalse()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		string location = "Test Store";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		Money taxAmount = new(5.00m);

		Receipt receipt1 = new(id1, location, date, taxAmount);
		Receipt receipt2 = new(id2, location, date, taxAmount);

		// Act & Assert
		Assert.False(receipt1 == receipt2);
		Assert.True(receipt1 != receipt2);
		Assert.False(receipt1.Equals(receipt2));
	}

	[Fact]
	public void Equals_DifferentLocation_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string location1 = "Test Store";
		string location2 = "Test Store 2";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		Money taxAmount = new(5.00m);

		Receipt receipt1 = new(id, location1, date, taxAmount);
		Receipt receipt2 = new(id, location2, date, taxAmount);

		// Act & Assert
		Assert.False(receipt1 == receipt2);
		Assert.True(receipt1 != receipt2);
		Assert.False(receipt1.Equals(receipt2));
	}

	[Fact]
	public void Equals_DifferentDate_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string location = "Test Store";
		DateOnly date1 = DateOnly.FromDateTime(DateTime.Today);
		DateOnly date2 = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
		Money taxAmount = new(5.00m);

		Receipt receipt1 = new(id, location, date1, taxAmount);
		Receipt receipt2 = new(id, location, date2, taxAmount);

		// Act & Assert
		Assert.False(receipt1 == receipt2);
		Assert.True(receipt1 != receipt2);
		Assert.False(receipt1.Equals(receipt2));
	}

	[Fact]
	public void Equals_DifferentTaxAmount_ReturnsFalse()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string location = "Test Store";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		Money taxAmount1 = new(5.00m);
		Money taxAmount2 = new(10.00m);

		Receipt receipt1 = new(id, location, date, taxAmount1);
		Receipt receipt2 = new(id, location, date, taxAmount2);

		// Act & Assert
		Assert.False(receipt1 == receipt2);
		Assert.True(receipt1 != receipt2);
		Assert.False(receipt1.Equals(receipt2));
	}

	[Fact]
	public void Equals_NullReceipt_ReturnsFalse()
	{
		// Arrange
		Receipt receipt = ReceiptGenerator.Generate();

		// Act & Assert
		Assert.False(receipt.Equals(null));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		Receipt receipt = ReceiptGenerator.Generate();

		// Act & Assert
		Assert.False(receipt.Equals((object?)null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		Receipt receipt = ReceiptGenerator.Generate();

		// Act & Assert
		Assert.False(receipt.Equals("not a receipt"));
	}

	[Fact]
	public void GetHashCode_SameReceipt_ReturnsSameHashCode()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string location = "Test Store";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		Money taxAmount = new(5.00m);

		Receipt receipt1 = new(id, location, date, taxAmount);
		Receipt receipt2 = new(id, location, date, taxAmount);

		// Act & Assert
		Assert.Equal(receipt1.GetHashCode(), receipt2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentReceipt_ReturnsDifferentHashCode()
	{
		// Arrange
		Guid id1 = Guid.NewGuid();
		Guid id2 = Guid.NewGuid();
		string location = "Test Store";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		Money taxAmount = new(5.00m);

		Receipt receipt1 = new(id1, location, date, taxAmount);
		Receipt receipt2 = new(id2, location, date, taxAmount);

		// Act & Assert
		Assert.NotEqual(receipt1.GetHashCode(), receipt2.GetHashCode());
	}
}