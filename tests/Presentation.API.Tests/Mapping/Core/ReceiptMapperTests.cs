using API.Generated.Dtos;
using API.Mapping.Core;
using Common;
using Domain;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class ReceiptMapperTests
{
	private readonly ReceiptMapper _mapper = new();

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAllPropertiesWithEmptyId()
	{
		// Arrange
		CreateReceiptRequest request = new()
		{
			Location = "Grocery Store",
			Date = new DateOnly(2025, 3, 15),
			TaxAmount = 5.75,
			Description = "Weekly groceries"
		};

		// Act
		Receipt actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("Grocery Store", actual.Location);
		Assert.Equal(new DateOnly(2025, 3, 15), actual.Date);
		Assert.Equal(5.75m, actual.TaxAmount.Amount);
		Assert.Equal(Currency.USD, actual.TaxAmount.Currency);
		Assert.Equal("Weekly groceries", actual.Description);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_MapsNullDescription()
	{
		// Arrange
		CreateReceiptRequest request = new()
		{
			Location = "Hardware Store",
			Date = new DateOnly(2025, 1, 10),
			TaxAmount = 3.25,
			Description = null
		};

		// Act
		Receipt actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Null(actual.Description);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAllPropertiesIncludingId()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateReceiptRequest request = new()
		{
			Id = expectedId,
			Location = "Updated Store",
			Date = new DateOnly(2025, 6, 20),
			TaxAmount = 12.50,
			Description = "Updated description"
		};

		// Act
		Receipt actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("Updated Store", actual.Location);
		Assert.Equal(new DateOnly(2025, 6, 20), actual.Date);
		Assert.Equal(12.50m, actual.TaxAmount.Amount);
		Assert.Equal(Currency.USD, actual.TaxAmount.Currency);
		Assert.Equal("Updated description", actual.Description);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsNullDescription()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateReceiptRequest request = new()
		{
			Id = expectedId,
			Location = "Some Store",
			Date = new DateOnly(2025, 2, 28),
			TaxAmount = 0.99,
			Description = null
		};

		// Act
		Receipt actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Null(actual.Description);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Receipt receipt = new(
			expectedId,
			"Electronics Store",
			new DateOnly(2025, 4, 10),
			new Money(8.99m, Currency.USD),
			"Monitor purchase"
		);

		// Act
		ReceiptResponse actual = _mapper.ToResponse(receipt);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("Electronics Store", actual.Location);
		Assert.Equal(new DateOnly(2025, 4, 10), actual.Date);
		Assert.Equal((double)8.99m, actual.TaxAmount);
		Assert.Equal("Monitor purchase", actual.Description);
	}

	[Fact]
	public void ToResponse_MapsNullDescription()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Receipt receipt = new(
			expectedId,
			"Office Supply",
			new DateOnly(2025, 5, 5),
			new Money(2.50m, Currency.USD)
		);

		// Act
		ReceiptResponse actual = _mapper.ToResponse(receipt);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Null(actual.Description);
	}

	[Fact]
	public void ToResponse_FlattensMoneyAmountToDouble()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Receipt receipt = new(
			expectedId,
			"Test Store",
			new DateOnly(2025, 1, 1),
			new Money(15.7531m, Currency.USD)
		);

		// Act
		ReceiptResponse actual = _mapper.ToResponse(receipt);

		// Assert
		Assert.Equal((double)15.7531m, actual.TaxAmount);
	}
}
