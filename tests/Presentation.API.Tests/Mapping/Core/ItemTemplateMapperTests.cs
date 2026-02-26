using API.Generated.Dtos;
using API.Mapping.Core;
using Domain;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class ItemTemplateMapperTests
{
	private readonly ItemTemplateMapper _mapper = new();

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAllPropertiesWithEmptyId()
	{
		// Arrange
		CreateItemTemplateRequest request = new()
		{
			Name = "Test Template",
			Description = "Test Description",
			DefaultCategory = "Groceries",
			DefaultSubcategory = "Produce",
			DefaultUnitPrice = 9.99,
			DefaultPricingMode = "quantity",
			DefaultItemCode = "ITEM-001"
		};

		// Act
		ItemTemplate actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("Test Template", actual.Name);
		Assert.Equal("Test Description", actual.Description);
		Assert.Equal("Groceries", actual.DefaultCategory);
		Assert.Equal("Produce", actual.DefaultSubcategory);
		Assert.NotNull(actual.DefaultUnitPrice);
		Assert.Equal(9.99m, actual.DefaultUnitPrice.Amount);
		Assert.Equal("quantity", actual.DefaultPricingMode);
		Assert.Equal("ITEM-001", actual.DefaultItemCode);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_NullOptionals_MapsCorrectly()
	{
		// Arrange
		CreateItemTemplateRequest request = new()
		{
			Name = "Simple Template"
		};

		// Act
		ItemTemplate actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("Simple Template", actual.Name);
		Assert.Null(actual.Description);
		Assert.Null(actual.DefaultCategory);
		Assert.Null(actual.DefaultSubcategory);
		Assert.Null(actual.DefaultUnitPrice);
		Assert.Null(actual.DefaultPricingMode);
		Assert.Null(actual.DefaultItemCode);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAllPropertiesIncludingId()
	{
		// Arrange
		Guid expected = Guid.NewGuid();
		UpdateItemTemplateRequest request = new()
		{
			Id = expected,
			Name = "Updated Template",
			Description = "Updated Description",
			DefaultCategory = "Electronics",
			DefaultSubcategory = "Phones",
			DefaultUnitPrice = 19.99,
			DefaultPricingMode = "flat",
			DefaultItemCode = "ITEM-002"
		};

		// Act
		ItemTemplate actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expected, actual.Id);
		Assert.Equal("Updated Template", actual.Name);
		Assert.Equal("Updated Description", actual.Description);
		Assert.Equal("Electronics", actual.DefaultCategory);
		Assert.Equal("Phones", actual.DefaultSubcategory);
		Assert.NotNull(actual.DefaultUnitPrice);
		Assert.Equal(19.99m, actual.DefaultUnitPrice.Amount);
		Assert.Equal("flat", actual.DefaultPricingMode);
		Assert.Equal("ITEM-002", actual.DefaultItemCode);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		ItemTemplate template = new(
			expectedId,
			"Response Template",
			"Groceries",
			"Produce",
			new Money(9.99m),
			"quantity",
			"ITEM-001",
			"A description"
		);

		// Act
		ItemTemplateResponse actual = _mapper.ToResponse(template);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("Response Template", actual.Name);
		Assert.Equal("A description", actual.Description);
		Assert.Equal("Groceries", actual.DefaultCategory);
		Assert.Equal("Produce", actual.DefaultSubcategory);
		Assert.Equal(9.99, actual.DefaultUnitPrice);
		Assert.Equal("quantity", actual.DefaultPricingMode);
		Assert.Equal("ITEM-001", actual.DefaultItemCode);
	}

	[Fact]
	public void ToResponse_NullUnitPrice_MapsToNull()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		ItemTemplate template = new(expectedId, "No Price Template");

		// Act
		ItemTemplateResponse actual = _mapper.ToResponse(template);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("No Price Template", actual.Name);
		Assert.Null(actual.DefaultUnitPrice);
	}
}
