using Domain.Core;

namespace Domain.Tests.Core;

public class SubcategoryTests
{
	[Fact]
	public void Constructor_ValidInput_CreatesSubcategory()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Subcategory";
		Guid categoryId = Guid.NewGuid();
		string description = "Test Description";

		// Act
		Subcategory subcategory = new(id, name, categoryId, description);

		// Assert
		Assert.Equal(id, subcategory.Id);
		Assert.Equal(name, subcategory.Name);
		Assert.Equal(categoryId, subcategory.CategoryId);
		Assert.Equal(description, subcategory.Description);
	}

	[Fact]
	public void Constructor_ValidInput_NullDescription_CreatesSubcategory()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Subcategory";
		Guid categoryId = Guid.NewGuid();

		// Act
		Subcategory subcategory = new(id, name, categoryId);

		// Assert
		Assert.Equal(id, subcategory.Id);
		Assert.Equal(name, subcategory.Name);
		Assert.Equal(categoryId, subcategory.CategoryId);
		Assert.Null(subcategory.Description);
	}

	[Fact]
	public void Constructor_EmptyId_CreatesSubcategoryWithEmptyId()
	{
		// Arrange
		string name = "Test Subcategory";
		Guid categoryId = Guid.NewGuid();
		string description = "Test Description";

		// Act
		Subcategory subcategory = new(Guid.Empty, name, categoryId, description);

		// Assert
		Assert.Equal(Guid.Empty, subcategory.Id);
		Assert.Equal(name, subcategory.Name);
		Assert.Equal(categoryId, subcategory.CategoryId);
		Assert.Equal(description, subcategory.Description);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public void Constructor_InvalidName_ThrowsArgumentException(string invalidName)
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid categoryId = Guid.NewGuid();

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Subcategory(id, invalidName, categoryId));
		Assert.StartsWith(Subcategory.NameCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Constructor_NullName_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		Guid categoryId = Guid.NewGuid();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		string name = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

		// Act & Assert
#pragma warning disable CS8604 // Possible null reference argument.
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Subcategory(id, name, categoryId));
#pragma warning restore CS8604 // Possible null reference argument.
		Assert.StartsWith(Subcategory.NameCannotBeEmpty, exception.Message);
	}

	[Fact]
	public void Constructor_EmptyCategoryId_ThrowsArgumentException()
	{
		// Arrange
		Guid id = Guid.NewGuid();
		string name = "Test Subcategory";

		// Act & Assert
		ArgumentException exception = Assert.Throws<ArgumentException>(() => new Subcategory(id, name, Guid.Empty));
		Assert.StartsWith(Subcategory.CategoryIdCannotBeEmpty, exception.Message);
	}
}
