using Common;
using Infrastructure.Entities.Core;

namespace Infrastructure.Tests.Entities.Core;

public class ItemTemplateEntityTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesItemTemplateEntity()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Template";
		string defaultCategory = "Groceries";
		string defaultSubcategory = "Produce";
		decimal defaultUnitPrice = 9.99m;
		Currency defaultUnitPriceCurrency = Currency.USD;
		string defaultPricingMode = "quantity";
		string defaultItemCode = "ITEM-001";
		string description = "Test Description";

		// Act
		ItemTemplateEntity entity = new()
		{
			Id = id,
			Name = name,
			DefaultCategory = defaultCategory,
			DefaultSubcategory = defaultSubcategory,
			DefaultUnitPrice = defaultUnitPrice,
			DefaultUnitPriceCurrency = defaultUnitPriceCurrency,
			DefaultPricingMode = defaultPricingMode,
			DefaultItemCode = defaultItemCode,
			Description = description
		};

		// Assert
		Assert.Equal(id, entity.Id);
		Assert.Equal(name, entity.Name);
		Assert.Equal(defaultCategory, entity.DefaultCategory);
		Assert.Equal(defaultSubcategory, entity.DefaultSubcategory);
		Assert.Equal(defaultUnitPrice, entity.DefaultUnitPrice);
		Assert.Equal(defaultUnitPriceCurrency, entity.DefaultUnitPriceCurrency);
		Assert.Equal(defaultPricingMode, entity.DefaultPricingMode);
		Assert.Equal(defaultItemCode, entity.DefaultItemCode);
		Assert.Equal(description, entity.Description);
	}

	[Fact]
	public void Constructor_NullableFields_DefaultToNull()
	{
		// Arrange & Act
		ItemTemplateEntity entity = new()
		{
			Id = Guid.NewGuid(),
			Name = "Test Template"
		};

		// Assert
		Assert.Null(entity.DefaultCategory);
		Assert.Null(entity.DefaultSubcategory);
		Assert.Null(entity.DefaultUnitPrice);
		Assert.Null(entity.DefaultUnitPriceCurrency);
		Assert.Null(entity.DefaultPricingMode);
		Assert.Null(entity.DefaultItemCode);
		Assert.Null(entity.Description);
		Assert.Null(entity.DeletedAt);
		Assert.Null(entity.DeletedByUserId);
		Assert.Null(entity.DeletedByApiKeyId);
	}
}
