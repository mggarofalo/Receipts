using API.Generated.Dtos;
using API.Mapping.Core;
using Common;
using Domain;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class AdjustmentMapperTests
{
	private readonly AdjustmentMapper _mapper = new();

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAllPropertiesWithEmptyId()
	{
		// Arrange
		CreateAdjustmentRequest request = new()
		{
			Type = "tip",
			Amount = 5.75,
			Description = null
		};

		// Act
		Adjustment actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal(AdjustmentType.Tip, actual.Type);
		Assert.Equal(5.75m, actual.Amount.Amount);
		Assert.Equal(Currency.USD, actual.Amount.Currency);
		Assert.Null(actual.Description);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_ParsesTypeIgnoringCase()
	{
		// Arrange
		CreateAdjustmentRequest request = new()
		{
			Type = "LoyaltyRedemption",
			Amount = 3.00
		};

		// Act
		Adjustment actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(AdjustmentType.LoyaltyRedemption, actual.Type);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_WithDescription()
	{
		// Arrange
		CreateAdjustmentRequest request = new()
		{
			Type = "other",
			Amount = 1.50,
			Description = "Employee discount"
		};

		// Act
		Adjustment actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(AdjustmentType.Other, actual.Type);
		Assert.Equal("Employee discount", actual.Description);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAllPropertiesIncludingId()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		UpdateAdjustmentRequest request = new()
		{
			Id = expectedId,
			Type = "discount",
			Amount = 10.00,
			Description = null
		};

		// Act
		Adjustment actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal(AdjustmentType.Discount, actual.Type);
		Assert.Equal(10.00m, actual.Amount.Amount);
		Assert.Null(actual.Description);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Adjustment adjustment = new(
			expectedId,
			AdjustmentType.Coupon,
			new Money(7.50m, Currency.USD),
			"Birthday coupon");
		adjustment.ReceiptId = Guid.NewGuid();

		// Act
		AdjustmentResponse actual = _mapper.ToResponse(adjustment);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal(adjustment.ReceiptId, actual.ReceiptId);
		Assert.Equal("Coupon", actual.Type);
		Assert.Equal((double)7.50m, actual.Amount);
		Assert.Equal("Birthday coupon", actual.Description);
	}

	[Fact]
	public void ToResponse_NullDescription_MapsToNull()
	{
		// Arrange
		Adjustment adjustment = new(
			Guid.NewGuid(),
			AdjustmentType.Rounding,
			new Money(0.01m, Currency.USD));

		// Act
		AdjustmentResponse actual = _mapper.ToResponse(adjustment);

		// Assert
		Assert.Null(actual.Description);
	}

	[Fact]
	public void ToResponse_FlattensMoneyAmountToDouble()
	{
		// Arrange
		Adjustment adjustment = new(
			Guid.NewGuid(),
			AdjustmentType.Tip,
			new Money(15.7531m, Currency.USD));

		// Act
		AdjustmentResponse actual = _mapper.ToResponse(adjustment);

		// Assert
		Assert.Equal((double)15.7531m, actual.Amount);
	}

	[Fact]
	public void RoundTrip_CreateRequest_ToDomain_ToResponse()
	{
		// Arrange
		CreateAdjustmentRequest request = new()
		{
			Type = "storeCredit",
			Amount = 25.00,
			Description = null
		};

		// Act
		Adjustment domain = _mapper.ToDomain(request);
		domain.ReceiptId = Guid.NewGuid();
		AdjustmentResponse response = _mapper.ToResponse(domain);

		// Assert
		Assert.Equal("StoreCredit", response.Type);
		Assert.Equal(25.00, response.Amount);
		Assert.Null(response.Description);
	}
}
