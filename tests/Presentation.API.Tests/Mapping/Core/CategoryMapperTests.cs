using API.Generated.Dtos;
using API.Mapping.Core;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class CategoryMapperTests
{
	private readonly CategoryMapper _mapper = new();

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAllPropertiesWithEmptyId()
	{
		// Arrange
		CreateCategoryRequest request = new()
		{
			Name = "Test Category",
			Description = "Test Description"
		};

		// Act
		Category actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("Test Category", actual.Name);
		Assert.Equal("Test Description", actual.Description);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_NullDescription()
	{
		// Arrange
		CreateCategoryRequest request = new()
		{
			Name = "Test Category",
			Description = null
		};

		// Act
		Category actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("Test Category", actual.Name);
		Assert.Null(actual.Description);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAllPropertiesIncludingId()
	{
		// Arrange
		Guid expected = Guid.NewGuid();
		UpdateCategoryRequest request = new()
		{
			Id = expected,
			Name = "Updated Category",
			Description = "Updated Description"
		};

		// Act
		Category actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expected, actual.Id);
		Assert.Equal("Updated Category", actual.Name);
		Assert.Equal("Updated Description", actual.Description);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Category category = new(expectedId, "Response Category", "Response Description");

		// Act
		CategoryResponse actual = _mapper.ToResponse(category);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("Response Category", actual.Name);
		Assert.Equal("Response Description", actual.Description);
	}
}
