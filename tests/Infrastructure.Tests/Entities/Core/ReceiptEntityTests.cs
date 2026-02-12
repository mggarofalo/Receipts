using Common;
using Infrastructure.Entities.Core;

namespace Infrastructure.Tests.Entities.Core;

public class ReceiptEntityTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesReceiptEntity()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string description = "Test Receipt";
		string location = "Test Store";
		DateOnly date = DateOnly.FromDateTime(DateTime.Today);
		decimal taxAmount = 5.00m;
		Currency taxAmountCurrency = Currency.USD;

		// Act
		ReceiptEntity receipt = new()
		{
			Id = id,
			Description = description,
			Location = location,
			Date = date,
			TaxAmount = taxAmount,
			TaxAmountCurrency = taxAmountCurrency
		};

		// Assert
		Assert.Equal(id, receipt.Id);
		Assert.Equal(description, receipt.Description);
		Assert.Equal(location, receipt.Location);
		Assert.Equal(date, receipt.Date);
		Assert.Equal(taxAmount, receipt.TaxAmount);
		Assert.Equal(taxAmountCurrency, receipt.TaxAmountCurrency);
	}
}
