using Common;
using Domain;
using Domain.Core;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Mapping;

public class AdjustmentMapperTests
{
	private readonly AdjustmentMapper _mapper = new();

	[Fact]
	public void ToEntity_MapsAllProperties()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Adjustment domain = new(id, AdjustmentType.Tip, new Money(5.00m, Currency.USD), "Nice service");

		// Act
		AdjustmentEntity entity = _mapper.ToEntity(domain);

		// Assert
		Assert.Equal(id, entity.Id);
		Assert.Equal(AdjustmentType.Tip, entity.Type);
		Assert.Equal(5.00m, entity.Amount);
		Assert.Equal(Currency.USD, entity.AmountCurrency);
		Assert.Equal("Nice service", entity.Description);
	}

	[Fact]
	public void ToEntity_NullDescription_MapsToNull()
	{
		// Arrange
		Adjustment domain = new(Guid.NewGuid(), AdjustmentType.Discount, new Money(-2.00m, Currency.USD));

		// Act
		AdjustmentEntity entity = _mapper.ToEntity(domain);

		// Assert
		Assert.Null(entity.Description);
	}

	[Fact]
	public void ToDomain_MapsAllProperties()
	{
		// Arrange
		AdjustmentEntity entity = new()
		{
			Id = Guid.NewGuid(),
			ReceiptId = Guid.NewGuid(),
			Type = AdjustmentType.Coupon,
			Amount = 7.50m,
			AmountCurrency = Currency.USD,
			Description = "Birthday coupon"
		};

		// Act
		Adjustment domain = _mapper.ToDomain(entity);

		// Assert
		Assert.Equal(entity.Id, domain.Id);
		Assert.Equal(AdjustmentType.Coupon, domain.Type);
		Assert.Equal(7.50m, domain.Amount.Amount);
		Assert.Equal(Currency.USD, domain.Amount.Currency);
		Assert.Equal("Birthday coupon", domain.Description);
	}

	[Fact]
	public void ToDomain_NullDescription_MapsToNull()
	{
		// Arrange
		AdjustmentEntity entity = AdjustmentEntityGenerator.Generate();
		entity.Description = null;

		// Act
		Adjustment domain = _mapper.ToDomain(entity);

		// Assert
		Assert.Null(domain.Description);
	}

	[Fact]
	public void RoundTrip_DomainToEntityToDomain_PreservesValues()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Adjustment original = new(id, AdjustmentType.StoreCredit, new Money(25.00m, Currency.USD), "Store credit");

		// Act
		AdjustmentEntity entity = _mapper.ToEntity(original);
		entity.ReceiptId = Guid.NewGuid();
		Adjustment roundTripped = _mapper.ToDomain(entity);

		// Assert
		roundTripped.Id.Should().Be(original.Id);
		roundTripped.Type.Should().Be(original.Type);
		roundTripped.Amount.Amount.Should().Be(original.Amount.Amount);
		roundTripped.Amount.Currency.Should().Be(original.Amount.Currency);
		roundTripped.Description.Should().Be(original.Description);
	}

	[Fact]
	public void RoundTrip_EntityToDomainToEntity_PreservesValues()
	{
		// Arrange
		AdjustmentEntity original = new()
		{
			Id = Guid.NewGuid(),
			ReceiptId = Guid.NewGuid(),
			Type = AdjustmentType.Rounding,
			Amount = 0.01m,
			AmountCurrency = Currency.USD,
			Description = null
		};

		// Act
		Adjustment domain = _mapper.ToDomain(original);
		AdjustmentEntity roundTripped = _mapper.ToEntity(domain);

		// Assert
		roundTripped.Id.Should().Be(original.Id);
		roundTripped.Type.Should().Be(original.Type);
		roundTripped.Amount.Should().Be(original.Amount);
		roundTripped.AmountCurrency.Should().Be(original.AmountCurrency);
		roundTripped.Description.Should().Be(original.Description);
	}

	[Theory]
	[InlineData(AdjustmentType.Tip)]
	[InlineData(AdjustmentType.Discount)]
	[InlineData(AdjustmentType.Rounding)]
	[InlineData(AdjustmentType.LoyaltyRedemption)]
	[InlineData(AdjustmentType.Coupon)]
	[InlineData(AdjustmentType.StoreCredit)]
	[InlineData(AdjustmentType.Other)]
	public void RoundTrip_AllAdjustmentTypes_PreservedThroughMapping(AdjustmentType type)
	{
		// Arrange
		string? description = type == AdjustmentType.Other ? "Custom adjustment" : null;
		Adjustment domain = new(Guid.NewGuid(), type, new Money(1.00m, Currency.USD), description);

		// Act
		AdjustmentEntity entity = _mapper.ToEntity(domain);
		entity.ReceiptId = Guid.NewGuid();
		Adjustment roundTripped = _mapper.ToDomain(entity);

		// Assert
		roundTripped.Type.Should().Be(type);
	}
}
