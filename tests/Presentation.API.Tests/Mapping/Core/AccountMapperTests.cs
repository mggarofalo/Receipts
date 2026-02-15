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
			AccountCode = "ACC-001",
			Name = "Test Checking Account",
			IsActive = true
		};

		// Act
		Account actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("ACC-001", actual.AccountCode);
		Assert.Equal("Test Checking Account", actual.Name);
		Assert.True(actual.IsActive);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_MapsInactiveAccount()
	{
		// Arrange
		CreateAccountRequest request = new()
		{
			AccountCode = "ACC-002",
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
			AccountCode = "ACC-UPD-001",
			Name = "Updated Account Name",
			IsActive = false
		};

		// Act
		Account actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expected, actual.Id);
		Assert.Equal("ACC-UPD-001", actual.AccountCode);
		Assert.Equal("Updated Account Name", actual.Name);
		Assert.False(actual.IsActive);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Account account = new(expectedId, "ACC-RES-001", "Response Account", true);

		// Act
		AccountResponse actual = _mapper.ToResponse(account);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("ACC-RES-001", actual.AccountCode);
		Assert.Equal("Response Account", actual.Name);
		Assert.True(actual.IsActive);
	}

	[Fact]
	public void ToResponse_MapsInactiveAccount()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Account account = new(expectedId, "ACC-RES-002", "Inactive Response Account", false);

		// Act
		AccountResponse actual = _mapper.ToResponse(account);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.False(actual.IsActive);
	}
}
