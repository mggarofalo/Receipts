using API.Generated.Dtos;
using API.Mapping.Core;
using Domain.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class CardMapperTests
{
	private readonly CardMapper _mapper = new();

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAllPropertiesWithEmptyId()
	{
		// Arrange
		Guid accountId = Guid.NewGuid();
		CreateCardRequest request = new()
		{
			CardCode = "ACC-001",
			Name = "Test Checking Account",
			IsActive = true,
			AccountId = accountId
		};

		// Act
		Card actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.Equal("ACC-001", actual.CardCode);
		Assert.Equal("Test Checking Account", actual.Name);
		Assert.True(actual.IsActive);
		Assert.Equal(accountId, actual.AccountId);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_MapsInactiveAccount()
	{
		// Arrange
		CreateCardRequest request = new()
		{
			CardCode = "ACC-002",
			Name = "Inactive Account",
			IsActive = false,
			AccountId = Guid.NewGuid()
		};

		// Act
		Card actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(Guid.Empty, actual.Id);
		Assert.False(actual.IsActive);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAllPropertiesIncludingId()
	{
		// Arrange
		Guid expected = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		UpdateCardRequest request = new()
		{
			Id = expected,
			CardCode = "ACC-UPD-001",
			Name = "Updated Card Name",
			IsActive = false,
			AccountId = accountId
		};

		// Act
		Card actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expected, actual.Id);
		Assert.Equal("ACC-UPD-001", actual.CardCode);
		Assert.Equal("Updated Card Name", actual.Name);
		Assert.False(actual.IsActive);
		Assert.Equal(accountId, actual.AccountId);
	}

	[Fact]
	public void ToResponse_MapsAllProperties()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Guid accountId = Guid.NewGuid();
		Card card = new(expectedId, "ACC-RES-001", "Response Account", accountId, true);

		// Act
		CardResponse actual = _mapper.ToResponse(card);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.Equal("ACC-RES-001", actual.CardCode);
		Assert.Equal("Response Account", actual.Name);
		Assert.True(actual.IsActive);
		Assert.Equal(accountId, actual.AccountId);
	}

	[Fact]
	public void ToResponse_MapsInactiveAccount()
	{
		// Arrange
		Guid expectedId = Guid.NewGuid();
		Card card = new(expectedId, "ACC-RES-002", "Inactive Response Account", Guid.NewGuid(), false);

		// Act
		CardResponse actual = _mapper.ToResponse(card);

		// Assert
		Assert.Equal(expectedId, actual.Id);
		Assert.False(actual.IsActive);
	}

	[Fact]
	public void ToDomain_FromCreateRequest_MapsAccountId()
	{
		// Arrange
		Guid expectedAccountId = Guid.NewGuid();
		CreateCardRequest request = new()
		{
			CardCode = "ACC-001",
			Name = "Child Card",
			IsActive = true,
			AccountId = expectedAccountId
		};

		// Act
		Card actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expectedAccountId, actual.AccountId);
	}

	[Fact]
	public void ToDomain_FromUpdateRequest_MapsAccountId()
	{
		// Arrange
		Guid expectedAccountId = Guid.NewGuid();
		UpdateCardRequest request = new()
		{
			Id = Guid.NewGuid(),
			CardCode = "ACC-001",
			Name = "Child Card",
			IsActive = true,
			AccountId = expectedAccountId
		};

		// Act
		Card actual = _mapper.ToDomain(request);

		// Assert
		Assert.Equal(expectedAccountId, actual.AccountId);
	}

	[Fact]
	public void ToResponse_IncludesAccountId()
	{
		// Arrange
		Guid expectedAccountId = Guid.NewGuid();
		Card card = new(Guid.NewGuid(), "CARD-A", "Primary Card", expectedAccountId, true);

		// Act
		CardResponse actual = _mapper.ToResponse(card);

		// Assert
		Assert.Equal(expectedAccountId, actual.AccountId);
	}
}
