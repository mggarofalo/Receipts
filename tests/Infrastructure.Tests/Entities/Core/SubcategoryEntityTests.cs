using Infrastructure.Entities.Core;

namespace Infrastructure.Tests.Entities.Core;

public class SubcategoryEntityTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesSubcategoryEntity()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Subcategory";
		Guid categoryId = Guid.NewGuid();
		string description = "Test Description";

		// Act
		SubcategoryEntity subcategory = new()
		{
			Id = id,
			Name = name,
			CategoryId = categoryId,
			Description = description
		};

		// Assert
		Assert.Equal(id, subcategory.Id);
		Assert.Equal(name, subcategory.Name);
		Assert.Equal(categoryId, subcategory.CategoryId);
		Assert.Equal(description, subcategory.Description);
	}
}
