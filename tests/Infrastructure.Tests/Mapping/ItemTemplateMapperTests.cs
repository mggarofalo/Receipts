using Common;
using Domain;
using Domain.Core;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;

namespace Infrastructure.Tests.Mapping;

public class ItemTemplateMapperTests
{
	private readonly ItemTemplateMapper _mapper = new();

	[Fact]
	public void ToEntity_WithDefaultUnitPrice_MapsAmountAndCurrency()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ItemTemplate domain = new(
			id,
			"Template With Price",
			"Groceries",
			"Produce",
			new Money(9.99m, Currency.USD),
			"quantity",
			"ITEM-001",
			"A description"
		);

		// Act
		ItemTemplateEntity entity = _mapper.ToEntity(domain);

		// Assert
		entity.Id.Should().Be(id);
		entity.Name.Should().Be("Template With Price");
		entity.DefaultUnitPrice.Should().Be(9.99m);
		entity.DefaultUnitPriceCurrency.Should().Be(Currency.USD);
		entity.DefaultCategory.Should().Be("Groceries");
		entity.DefaultSubcategory.Should().Be("Produce");
		entity.DefaultPricingMode.Should().Be("quantity");
		entity.DefaultItemCode.Should().Be("ITEM-001");
		entity.Description.Should().Be("A description");
	}

	[Fact]
	public void ToEntity_WithNullDefaultUnitPrice_MapsNullAmountAndCurrency()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ItemTemplate domain = new(id, "No Price Template");

		// Act
		ItemTemplateEntity entity = _mapper.ToEntity(domain);

		// Assert
		entity.Id.Should().Be(id);
		entity.Name.Should().Be("No Price Template");
		entity.DefaultUnitPrice.Should().BeNull();
		entity.DefaultUnitPriceCurrency.Should().BeNull();
	}

	[Fact]
	public void ToDomain_WithBothPriceAndCurrency_ReconstructsMoney()
	{
		// Arrange
		ItemTemplateEntity entity = new()
		{
			Id = Guid.NewGuid(),
			Name = "Entity With Price",
			DefaultUnitPrice = 15.50m,
			DefaultUnitPriceCurrency = Currency.USD,
			DefaultCategory = "Electronics",
			DefaultSubcategory = "Cables",
			DefaultPricingMode = "flat",
			DefaultItemCode = "ITEM-002",
			Description = "Desc"
		};

		// Act
		ItemTemplate domain = _mapper.ToDomain(entity);

		// Assert
		domain.DefaultUnitPrice.Should().NotBeNull();
		domain.DefaultUnitPrice!.Amount.Should().Be(15.50m);
		domain.DefaultUnitPrice.Currency.Should().Be(Currency.USD);
	}

	[Fact]
	public void ToDomain_WithNullPrice_ReturnsNullMoney()
	{
		// Arrange
		ItemTemplateEntity entity = new()
		{
			Id = Guid.NewGuid(),
			Name = "Entity Without Price",
			DefaultUnitPrice = null,
			DefaultUnitPriceCurrency = null,
		};

		// Act
		ItemTemplate domain = _mapper.ToDomain(entity);

		// Assert
		domain.DefaultUnitPrice.Should().BeNull();
	}

	[Fact]
	public void ToDomain_WithPriceButNoCurrency_ReturnsNullMoney()
	{
		// Arrange — only amount set, currency is null
		ItemTemplateEntity entity = new()
		{
			Id = Guid.NewGuid(),
			Name = "Price Only Template",
			DefaultUnitPrice = 10.00m,
			DefaultUnitPriceCurrency = null,
		};

		// Act
		ItemTemplate domain = _mapper.ToDomain(entity);

		// Assert — compound condition fails, Money is null
		domain.DefaultUnitPrice.Should().BeNull();
	}

	[Fact]
	public void ToDomain_WithCurrencyButNoPrice_ReturnsNullMoney()
	{
		// Arrange — only currency set, amount is null
		ItemTemplateEntity entity = new()
		{
			Id = Guid.NewGuid(),
			Name = "Currency Only Template",
			DefaultUnitPrice = null,
			DefaultUnitPriceCurrency = Currency.USD,
		};

		// Act
		ItemTemplate domain = _mapper.ToDomain(entity);

		// Assert — compound condition fails, Money is null
		domain.DefaultUnitPrice.Should().BeNull();
	}

	[Fact]
	public void RoundTrip_DomainToEntityToDomain_PreservesValues()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ItemTemplate original = new(
			id,
			"Round Trip Template",
			"Groceries",
			"Produce",
			new Money(12.34m, Currency.USD),
			"quantity",
			"ITEM-RT",
			"Round trip description"
		);

		// Act
		ItemTemplateEntity entity = _mapper.ToEntity(original);
		ItemTemplate roundTripped = _mapper.ToDomain(entity);

		// Assert
		roundTripped.Id.Should().Be(original.Id);
		roundTripped.Name.Should().Be(original.Name);
		roundTripped.DefaultCategory.Should().Be(original.DefaultCategory);
		roundTripped.DefaultSubcategory.Should().Be(original.DefaultSubcategory);
		roundTripped.DefaultUnitPrice!.Amount.Should().Be(original.DefaultUnitPrice!.Amount);
		roundTripped.DefaultUnitPrice.Currency.Should().Be(original.DefaultUnitPrice.Currency);
		roundTripped.DefaultPricingMode.Should().Be(original.DefaultPricingMode);
		roundTripped.DefaultItemCode.Should().Be(original.DefaultItemCode);
		roundTripped.Description.Should().Be(original.Description);
	}

	[Fact]
	public void RoundTrip_NullPrice_PreservesNull()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		ItemTemplate original = new(id, "No Price Round Trip");

		// Act
		ItemTemplateEntity entity = _mapper.ToEntity(original);
		ItemTemplate roundTripped = _mapper.ToDomain(entity);

		// Assert
		roundTripped.DefaultUnitPrice.Should().BeNull();
	}
}
