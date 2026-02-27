using Domain.Core;

namespace Domain.Tests.Core;

public class CategoryTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesCategory()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Category";
		string description = "Test Description";

		// Act
		Category category = new(id, name, description);

		// Assert
		Assert.Equal(id, category.Id);
		Assert.Equal(name, category.Name);
		Assert.Equal(description, category.Description);
	}

	[Fact]
	public void Constructor_ValidInput_NullDescription_CreatesCategory()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Category";

		// Act
		Category category = new(id, name);

		// Assert
		Assert.Equal(id, category.Id);
		Assert.Equal(name, category.Name);
		Assert.Null(category.Description);
	}

	[Fact]
	public void Constructor_EmptyId_CreatesCategoryWithEmptyId()
	{
		// Arrange
		string name = "Test Category";
		string description = "Test Description";

		// Act
		Category category = new(Guid.Empty, name, description);

		// Assert
		Assert.Equal(Guid.Empty, category.Id);
		Assert.Equal(name, category.Name);
		Assert.Equal(description, category.Description);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
	{
		// Arrange
		Guid id = Guid.NewGuid();

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Category(id, invalidName));
		Assert.StartsWith(Category.NameCannotBeEmpty, exception.Message);
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
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Category(id, name));
#pragma warning restore CS8604 // Possible null reference argument.
		Assert.StartsWith(Category.NameCannotBeEmpty, exception.Message);
	}
}
