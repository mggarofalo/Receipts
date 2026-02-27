using Domain;
using Domain.Core;

namespace Domain.Tests.Core;

public class ItemTemplateTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesItemTemplate()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Template";
		string description = "Test Description";
		string defaultCategory = "Groceries";
		string defaultSubcategory = "Produce";
		Money defaultUnitPrice = new(9.99m);
		string defaultPricingMode = "quantity";
		string defaultItemCode = "ITEM-001";

		// Act
		ItemTemplate itemTemplate = new(id, name, defaultCategory, defaultSubcategory, defaultUnitPrice, defaultPricingMode, defaultItemCode, description);

		// Assert
		Assert.Equal(id, itemTemplate.Id);
		Assert.Equal(name, itemTemplate.Name);
		Assert.Equal(description, itemTemplate.Description);
		Assert.Equal(defaultCategory, itemTemplate.DefaultCategory);
		Assert.Equal(defaultSubcategory, itemTemplate.DefaultSubcategory);
		Assert.Equal(defaultUnitPrice, itemTemplate.DefaultUnitPrice);
		Assert.Equal(defaultPricingMode, itemTemplate.DefaultPricingMode);
		Assert.Equal(defaultItemCode, itemTemplate.DefaultItemCode);
	}

	[Fact]
	public void Constructor_ValidInput_NullOptionals_CreatesItemTemplate()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Template";

		// Act
		ItemTemplate itemTemplate = new(id, name);

		// Assert
		Assert.Equal(id, itemTemplate.Id);
		Assert.Equal(name, itemTemplate.Name);
		Assert.Null(itemTemplate.Description);
		Assert.Null(itemTemplate.DefaultCategory);
		Assert.Null(itemTemplate.DefaultSubcategory);
		Assert.Null(itemTemplate.DefaultUnitPrice);
		Assert.Null(itemTemplate.DefaultPricingMode);
		Assert.Null(itemTemplate.DefaultItemCode);
	}

	[Fact]
	public void Constructor_EmptyId_CreatesItemTemplateWithEmptyId()
	{
		// Arrange
		string name = "Test Template";

		// Act
		ItemTemplate itemTemplate = new(Guid.Empty, name);

		// Assert
		Assert.Equal(Guid.Empty, itemTemplate.Id);
		Assert.Equal(name, itemTemplate.Name);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
	{
		// Arrange
		Guid id = Guid.NewGuid();

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ItemTemplate(id, invalidName));
		Assert.StartsWith(ItemTemplate.NameCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Constructor_NullName_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		string name = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

		// Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ItemTemplate(id, name));
#pragma warning restore CS8604 // Possible null reference argument.
		Assert.StartsWith(ItemTemplate.NameCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Constructor_ValidPricingMode_Quantity_CreatesItemTemplate()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Template";

		// Act
		ItemTemplate itemTemplate = new(id, name, defaultPricingMode: "quantity");

		// Assert
		Assert.Equal("quantity", itemTemplate.DefaultPricingMode);
	}

	[Fact]
	public void Constructor_ValidPricingMode_Flat_CreatesItemTemplate()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Template";

		// Act
		ItemTemplate itemTemplate = new(id, name, defaultPricingMode: "flat");

		// Assert
		Assert.Equal("flat", itemTemplate.DefaultPricingMode);
	}

	[Fact]
	public void Constructor_InvalidPricingMode_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Template";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new ItemTemplate(id, name, defaultPricingMode: "invalid"));
		Assert.StartsWith(ItemTemplate.DefaultPricingModeInvalid, exception.Message);
	}
}
