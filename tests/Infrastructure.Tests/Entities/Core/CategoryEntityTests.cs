using Infrastructure.Entities.Core;

namespace Infrastructure.Tests.Entities.Core;

public class CategoryEntityTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesCategoryEntity()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Category";
		string description = "Test Description";

		// Act
		CategoryEntity category = new()
		{
			Id = id,
			Name = name,
			Description = description
		};

		// Assert
		Assert.Equal(id, category.Id);
		Assert.Equal(name, category.Name);
		Assert.Equal(description, category.Description);
	}
}
