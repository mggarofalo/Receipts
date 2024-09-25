using Common;
using Infrastructure.Entities.Core;
using SampleData.Entities;

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

	[Fact]
	public void Equals_SameReceiptEntity_ReturnsTrue()
	{
		// Arrange
		ReceiptEntity receipt1 = ReceiptEntityGenerator.Generate();
		ReceiptEntity receipt2 = new()
		{
			Id = receipt1.Id,
			Description = receipt1.Description,
			Location = receipt1.Location,
			Date = receipt1.Date,
			TaxAmount = receipt1.TaxAmount,
			TaxAmountCurrency = receipt1.TaxAmountCurrency
		};

		// Act & Assert
		Assert.Equal(receipt1, receipt2);
	}

	[Fact]
	public void Equals_DifferentReceiptEntity_ReturnsFalse()
	{
		// Arrange
		ReceiptEntity receipt1 = ReceiptEntityGenerator.Generate();
		ReceiptEntity receipt2 = ReceiptEntityGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receipt1, receipt2);
	}

	[Fact]
	public void Equals_NullReceiptEntity_ReturnsFalse()
	{
		// Arrange
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();

		// Act & Assert
		Assert.False(receipt.Equals(null));
	}

	[Fact]
	public void Equals_DifferentType_ReturnsFalse()
	{
		// Arrange
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();

		// Act & Assert
		Assert.False(receipt.Equals("not a receipt entity"));
	}

	[Fact]
	public void Equals_NullObject_ReturnsFalse()
	{
		// Arrange
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();

		// Act & Assert
		Assert.False(receipt.Equals((object?)null));
	}

	[Fact]
	public void GetHashCode_SameReceiptEntity_ReturnsSameHashCode()
	{
		// Arrange
		ReceiptEntity receipt1 = ReceiptEntityGenerator.Generate();
		ReceiptEntity receipt2 = new()
		{
			Id = receipt1.Id,
			Description = receipt1.Description,
			Location = receipt1.Location,
			Date = receipt1.Date,
			TaxAmount = receipt1.TaxAmount,
			TaxAmountCurrency = receipt1.TaxAmountCurrency
		};

		// Act & Assert
		Assert.Equal(receipt1.GetHashCode(), receipt2.GetHashCode());
	}

	[Fact]
	public void GetHashCode_DifferentReceiptEntity_ReturnsDifferentHashCode()
	{
		// Arrange
		ReceiptEntity receipt1 = ReceiptEntityGenerator.Generate();
		ReceiptEntity receipt2 = ReceiptEntityGenerator.Generate();

		// Act & Assert
		Assert.NotEqual(receipt1.GetHashCode(), receipt2.GetHashCode());
	}

	[Fact]
	public void OperatorEqual_SameReceiptEntity_ReturnsTrue()
	{
		// Arrange
		ReceiptEntity receipt1 = ReceiptEntityGenerator.Generate();
		ReceiptEntity receipt2 = receipt1;

		// Act
		bool result = receipt1 == receipt2;

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void OperatorEqual_DifferentReceiptEntity_ReturnsFalse()
	{
		// Arrange
		ReceiptEntity receipt1 = ReceiptEntityGenerator.Generate();
		ReceiptEntity receipt2 = ReceiptEntityGenerator.Generate();

		// Act
		bool result = receipt1 == receipt2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_SameReceiptEntity_ReturnsFalse()
	{
		// Arrange
		ReceiptEntity receipt1 = ReceiptEntityGenerator.Generate();
		ReceiptEntity receipt2 = receipt1;

		// Act
		bool result = receipt1 != receipt2;

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void OperatorNotEqual_DifferentReceiptEntity_ReturnsTrue()
	{
		// Arrange
		ReceiptEntity receipt1 = ReceiptEntityGenerator.Generate();
		ReceiptEntity receipt2 = ReceiptEntityGenerator.Generate();

		// Act
		bool result = receipt1 != receipt2;

		// Assert
		Assert.True(result);
	}
}