using Shared.ViewModels.Core;

namespace Presentation.Shared.Tests.ViewModels.Core;

public class ReceiptVMTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesReceiptVM()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string description = "Test Description";
		string location = "Test Location";
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);
		decimal taxAmount = 10.0m;

		// Act
		ReceiptVM receiptVM = new()
		{
			Id = id,
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};

		// Assert
		Assert.Equal(id, receiptVM.Id);
		Assert.Equal(description, receiptVM.Description);
		Assert.Equal(location, receiptVM.Location);
		Assert.Equal(date, receiptVM.Date);
		Assert.Equal(taxAmount, receiptVM.TaxAmount);
	}

	[Fact]
	public void Constructor_NullId_CreatesReceiptVMWithNullId()
	{
		// Arrange
		string description = "Test Description";
		string location = "Test Location";
		DateOnly date = DateOnly.FromDateTime(DateTime.Now);
		decimal taxAmount = 10.0m;

		// Act
		ReceiptVM receiptVM = new()
		{
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount
		};

		// Assert
		Assert.Null(receiptVM.Id);
	}
}
