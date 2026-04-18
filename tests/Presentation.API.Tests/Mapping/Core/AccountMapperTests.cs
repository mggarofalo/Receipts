using API.Generated.Dtos;
using API.Mapping.Core;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class AccountMapperTests
{
	private readonly AccountMapper _mapper = new();

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAllPropertiesWithEmptyId()
	{
		// Arrange
		CreateAccountRequest request = new()
		{
			Name = "Test Checking Account",
			IsActive = true
		};

		// Act
		Account actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("Test Checking Account", actual.Name);
		Assert.True(actual.IsActive);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_MapsInactiveAccount()
	{
		// Arrange
		CreateAccountRequest request = new()
		{
			Name = "Inactive Account",
			IsActive = false
		};

		// Act
		Account actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.False(actual.IsActive);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAllPropertiesIncludingId()
	{
		// Arrange
		Guid expected = Guid.NewGuid();
		UpdateAccountRequest request = new()
		{
			Id = expected,
			Name = "Updated Account Name",
			IsActive = false
		};

		// Act
		Account actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expected, actual.Id);
		Assert.Equal("Updated Account Name", actual.Name);
		Assert.False(actual.IsActive);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Account account = new(expectedId, "Response Account", true);

		// Act
		AccountResponse actual = _mapper.ToResponse(account);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("Response Account", actual.Name);
		Assert.True(actual.IsActive);
	}

	[Fact]
	public void ToResponse_MapsInactiveAccount()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Account account = new(expectedId, "Inactive Response Account", false);

		// Act
		AccountResponse actual = _mapper.ToResponse(account);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.False(actual.IsActive);
	}
}
