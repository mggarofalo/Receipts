using API.Generated.Dtos;
using API.Mapping.Core;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class SubcategoryMapperTests
{
	private readonly SubcategoryMapper _mapper = new();

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAllPropertiesWithEmptyId()
	{
		// Arrange
		Guid categoryId = Guid.NewGuid();
		CreateSubcategoryRequest request = new()
		{
			Name = "Test Subcategory",
			CategoryId = categoryId,
			Description = "Test Description"
		};

		// Act
		Subcategory actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("Test Subcategory", actual.Name);
		Assert.Equal(categoryId, actual.CategoryId);
		Assert.Equal("Test Description", actual.Description);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_NullDescription()
	{
		// Arrange
		Guid categoryId = Guid.NewGuid();
		CreateSubcategoryRequest request = new()
		{
			Name = "Test Subcategory",
			CategoryId = categoryId,
			Description = null
		};

		// Act
		Subcategory actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("Test Subcategory", actual.Name);
		Assert.Equal(categoryId, actual.CategoryId);
		Assert.Null(actual.Description);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAllPropertiesIncludingId()
	{
		// Arrange
		Guid expected = Guid.NewGuid();
		Guid categoryId = Guid.NewGuid();
		UpdateSubcategoryRequest request = new()
		{
			Id = expected,
			Name = "Updated Subcategory",
			CategoryId = categoryId,
			Description = "Updated Description"
		};

		// Act
		Subcategory actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expected, actual.Id);
		Assert.Equal("Updated Subcategory", actual.Name);
		Assert.Equal(categoryId, actual.CategoryId);
		Assert.Equal("Updated Description", actual.Description);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Guid categoryId = Guid.NewGuid();
		Subcategory subcategory = new(expectedId, "Response Subcategory", categoryId, "Response Description");

		// Act
		SubcategoryResponse actual = _mapper.ToResponse(subcategory);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("Response Subcategory", actual.Name);
		Assert.Equal(categoryId, actual.CategoryId);
		Assert.Equal("Response Description", actual.Description);
	}
}
