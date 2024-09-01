using Domain.Core;

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
		Assert.Empty(receipt.Items);
		Assert.Empty(receipt.Transactions);
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
	public void AddTransaction_ValidTransaction_AddsToTransactions()
	{
		// Arrange
		Receipt receipt = new(Guid.NewGuid(), "Test Store", DateOnly.FromDateTime(DateTime.Today), new Money(5.00m));
		Transaction transaction = new(Guid.NewGuid(), receipt.Id!.Value, Guid.NewGuid(), new Money(100.00m), DateOnly.FromDateTime(DateTime.Today));

		// Act
		receipt.AddTransaction(transaction);

		// Assert
		Assert.Single(receipt.Transactions);
		Assert.Contains(transaction, receipt.Transactions);
	}

	[Fact]
	public void AddTransaction_NullTransaction_ThrowsArgumentNullException()
	{
		// Arrange
		Receipt receipt = new(Guid.NewGuid(), "Test Store", DateOnly.FromDateTime(DateTime.Today), new Money(5.00m));

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => receipt.AddTransaction(null!));
	}

	[Fact]
	public void AddItem_ValidItem_AddsToItems()
	{
		// Arrange
		Receipt receipt = new(Guid.NewGuid(), "Test Store", DateOnly.FromDateTime(DateTime.Today), new Money(5.00m));
		ReceiptItem item = new(Guid.NewGuid(), "ITEM001", "Test Item", 1, new Money(10.00m), new Money(10.00m), "Test Category", "Test Subcategory");

		// Act
		receipt.AddItem(item);

		// Assert
		Assert.Single(receipt.Items);
		Assert.Contains(item, receipt.Items);
	}

	[Fact]
	public void AddItem_NullItem_ThrowsArgumentNullException()
	{
		// Arrange
		Receipt receipt = new(Guid.NewGuid(), "Test Store", DateOnly.FromDateTime(DateTime.Today), new Money(5.00m));

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => receipt.AddItem(null!));
	}
}