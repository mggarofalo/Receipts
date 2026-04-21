using API.Generated.Dtos;
using API.Mapping.Core;
using Common;
using Domain;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class ReceiptItemMapperTests
{
	private readonly ReceiptItemMapper _mapper = new();

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAllPropertiesWithEmptyId()
	{
		// Arrange
		CreateReceiptItemRequest request = new()
		{
			ReceiptItemCode = "ITEM-001",
			Description = "Organic Apples",
			Quantity = 3.0,
			UnitPrice = 2.49,
			Category = "Groceries",
			Subcategory = "Produce"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("ITEM-001", actual.ReceiptItemCode);
		Assert.Equal("Organic Apples", actual.Description);
		Assert.Equal(3.0m, actual.Quantity);
		Assert.Equal(2.49m, actual.UnitPrice.Amount);
		Assert.Equal(Currency.USD, actual.UnitPrice.Currency);
		Assert.Equal("Groceries", actual.Category);
		Assert.Equal("Produce", actual.Subcategory);
		Assert.Equal(PricingMode.Quantity, actual.PricingMode);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_CalculatesTotalAmountWithFloor()
	{
		// Arrange
		CreateReceiptItemRequest request = new()
		{
			ReceiptItemCode = "ITEM-002",
			Description = "Widgets",
			Quantity = 3.0,
			UnitPrice = 1.333,
			Category = "Parts",
			Subcategory = "Hardware"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		decimal expectedTotal = Math.Floor(3.0m * 1.333m * 100) / 100;
		Assert.Equal(expectedTotal, actual.TotalAmount.Amount);
		Assert.Equal(Currency.USD, actual.TotalAmount.Currency);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_TotalAmountFloorRoundsDown()
	{
		// Arrange - values that would round up with normal rounding
		CreateReceiptItemRequest request = new()
		{
			ReceiptItemCode = "ITEM-003",
			Description = "Precision Item",
			Quantity = 7.0,
			UnitPrice = 1.999,
			Category = "Test",
			Subcategory = "Precision"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		decimal expectedTotal = Math.Floor(7.0m * 1.999m * 100) / 100;
		Assert.Equal(expectedTotal, actual.TotalAmount.Amount);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAllPropertiesIncludingId()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateReceiptItemRequest request = new()
		{
			Id = expectedId,
			ReceiptItemCode = "ITEM-UPD-001",
			Description = "Updated Bananas",
			Quantity = 5.0,
			UnitPrice = 0.79,
			Category = "Groceries",
			Subcategory = "Fruit"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("ITEM-UPD-001", actual.ReceiptItemCode);
		Assert.Equal("Updated Bananas", actual.Description);
		Assert.Equal(5.0m, actual.Quantity);
		Assert.Equal(0.79m, actual.UnitPrice.Amount);
		Assert.Equal(Currency.USD, actual.UnitPrice.Currency);
		Assert.Equal("Groceries", actual.Category);
		Assert.Equal("Fruit", actual.Subcategory);
		Assert.Equal(PricingMode.Quantity, actual.PricingMode);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_CalculatesTotalAmountWithFloor()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateReceiptItemRequest request = new()
		{
			Id = expectedId,
			ReceiptItemCode = "ITEM-UPD-002",
			Description = "Updated Widgets",
			Quantity = 4.0,
			UnitPrice = 2.337,
			Category = "Parts",
			Subcategory = "Hardware"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		decimal expectedTotal = Math.Floor(4.0m * 2.337m * 100) / 100;
		Assert.Equal(expectedTotal, actual.TotalAmount.Amount);
		Assert.Equal(Currency.USD, actual.TotalAmount.Currency);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		ReceiptItem item = new(
			expectedId,
			"ITEM-RES-001",
			"Response Item",
			2.5m,
			new Money(10.99m, Currency.USD),
			new Money(27.47m, Currency.USD),
			"Electronics",
			"Cables"
		);

		// Act
		ReceiptItemResponse actual = _mapper.ToResponse(item);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("ITEM-RES-001", actual.ReceiptItemCode);
		Assert.Equal("Response Item", actual.Description);
		Assert.Equal((double)2.5m, actual.Quantity);
		Assert.Equal((double)10.99m, actual.UnitPrice);
		Assert.Equal("Electronics", actual.Category);
		Assert.Equal("Cables", actual.Subcategory);
		Assert.Equal("quantity", actual.PricingMode);
	}

	[Fact]
	public void ToResponse_FlattensMoneyUnitPriceToDouble()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		ReceiptItem item = new(
			expectedId,
			"ITEM-RES-002",
			"Price Test Item",
			1.0m,
			new Money(33.4567m, Currency.USD),
			new Money(33.45m, Currency.USD),
			"Test",
			"Price"
		);

		// Act
		ReceiptItemResponse actual = _mapper.ToResponse(item);

		// Assert
		Assert.Equal((double)33.4567m, actual.UnitPrice);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_FlatPricingMode_MapsPricingMode()
	{
		// Arrange
		CreateReceiptItemRequest request = new()
		{
			ReceiptItemCode = "ITEM-FLAT-001",
			Description = "Flat Price Item",
			Quantity = 1.0,
			UnitPrice = 14.97,
			Category = "Groceries",
			Subcategory = "Produce",
			PricingMode = "flat"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(PricingMode.Flat, actual.PricingMode);
		Assert.Equal(1.0m, actual.Quantity);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_NullPricingMode_DefaultsToQuantity()
	{
		// Arrange
		CreateReceiptItemRequest request = new()
		{
			ReceiptItemCode = "ITEM-NULL-001",
			Description = "Null PricingMode Item",
			Quantity = 2.0,
			UnitPrice = 5.00,
			Category = "Test",
			Subcategory = "Default"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(PricingMode.Quantity, actual.PricingMode);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_FlatPricingMode_MapsPricingMode()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateReceiptItemRequest request = new()
		{
			Id = expectedId,
			ReceiptItemCode = "ITEM-FLAT-UPD-001",
			Description = "Updated Flat Item",
			Quantity = 1.0,
			UnitPrice = 25.00,
			Category = "Electronics",
			Subcategory = "Cables",
			PricingMode = "flat"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(PricingMode.Flat, actual.PricingMode);
		Assert.Equal(expectedId, actual.Id);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_NullReceiptItemCodeAndSubcategory_MapsAsNull()
	{
		// Arrange
		CreateReceiptItemRequest request = new()
		{
			ReceiptItemCode = null,
			Description = "No Code Item",
			Quantity = 1.0,
			UnitPrice = 5.00,
			Category = "Test",
			Subcategory = null
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Null(actual.ReceiptItemCode);
		Assert.Null(actual.Subcategory);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_NullReceiptItemCodeAndSubcategory_MapsAsNull()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateReceiptItemRequest request = new()
		{
			Id = expectedId,
			ReceiptItemCode = null,
			Description = "No Code Item",
			Quantity = 1.0,
			UnitPrice = 5.00,
			Category = "Test",
			Subcategory = null
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Null(actual.ReceiptItemCode);
		Assert.Null(actual.Subcategory);
	}

	[Fact]
	public void ToResponse_NullReceiptItemCodeAndSubcategory_MapsAsNull()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		ReceiptItem item = new(
			expectedId,
			null,
			"No Code Item",
			1.0m,
			new Money(5.00m, Currency.USD),
			new Money(5.00m, Currency.USD),
			"Test",
			null
		);

		// Act
		ReceiptItemResponse actual = _mapper.ToResponse(item);

		// Assert
		Assert.Null(actual.ReceiptItemCode);
		Assert.Null(actual.Subcategory);
	}

	[Fact]
	public void ToResponse_MapsPricingMode()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		ReceiptItem item = new(
			expectedId,
			"ITEM-RES-003",
			"Flat Response Item",
			1.0m,
			new Money(15.00m, Currency.USD),
			new Money(15.00m, Currency.USD),
			"Test",
			"Flat",
			PricingMode.Flat
		);

		// Act
		ReceiptItemResponse actual = _mapper.ToResponse(item);

		// Assert
		Assert.Equal("flat", actual.PricingMode);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_InvalidPricingMode_DefaultsToQuantity()
	{
		// Arrange
		CreateReceiptItemRequest request = new()
		{
			ReceiptItemCode = "ITEM-INV-001",
			Description = "Invalid PricingMode Item",
			Quantity = 1.0,
			UnitPrice = 5.00,
			Category = "Test",
			Subcategory = "Invalid",
			PricingMode = "nonexistent_mode"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(PricingMode.Quantity, actual.PricingMode);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_InvalidPricingMode_DefaultsToQuantity()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateReceiptItemRequest request = new()
		{
			Id = expectedId,
			ReceiptItemCode = "ITEM-INV-UPD-001",
			Description = "Invalid PricingMode Update",
			Quantity = 1.0,
			UnitPrice = 5.00,
			Category = "Test",
			Subcategory = "Invalid",
			PricingMode = "nonexistent_mode"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(PricingMode.Quantity, actual.PricingMode);
		Assert.Equal(expectedId, actual.Id);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_NullPricingMode_DefaultsToQuantity()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateReceiptItemRequest request = new()
		{
			Id = expectedId,
			ReceiptItemCode = "ITEM-NULL-UPD-001",
			Description = "Null PricingMode Update",
			Quantity = 2.0,
			UnitPrice = 5.00,
			Category = "Test",
			Subcategory = "Default"
		};

		// Act
		ReceiptItem actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(PricingMode.Quantity, actual.PricingMode);
	}

	[Fact]
	public void ToResponse_MapsNormalizedDescriptionFields_WhenPopulated()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Guid expectedNormalizedId = Guid.NewGuid();
		ReceiptItem item = new(
			expectedId,
			"ITEM-NORM-001",
			"Red Seedless Grapes 2LB",
			1.0m,
			new Money(5.99m, Currency.USD),
			new Money(5.99m, Currency.USD),
			"Groceries",
			"Produce"
		)
		{
			NormalizedDescriptionId = expectedNormalizedId,
			NormalizedDescriptionName = "Grapes",
			NormalizedDescriptionMatchScore = 0.92
		};

		// Act
		ReceiptItemResponse actual = _mapper.ToResponse(item);

		// Assert
		Assert.Equal(expectedNormalizedId, actual.NormalizedDescriptionId);
		Assert.Equal("Grapes", actual.NormalizedDescriptionName);
		Assert.Equal(0.92, actual.NormalizedDescriptionMatchScore);
	}

	[Fact]
	public void ToResponse_MapsNullNormalizedDescriptionFields_WhenUnresolved()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		ReceiptItem item = new(
			expectedId,
			"ITEM-NORM-002",
			"Unresolved Item",
			1.0m,
			new Money(1.00m, Currency.USD),
			new Money(1.00m, Currency.USD),
			"Test",
			"Unresolved"
		);

		// Act
		ReceiptItemResponse actual = _mapper.ToResponse(item);

		// Assert
		Assert.Null(actual.NormalizedDescriptionId);
		Assert.Null(actual.NormalizedDescriptionName);
		Assert.Null(actual.NormalizedDescriptionMatchScore);
	}
}
