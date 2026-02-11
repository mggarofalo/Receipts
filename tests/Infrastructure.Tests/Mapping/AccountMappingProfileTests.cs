using AutoMapper;
using Domain.Core;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using SampleData.Domain.Core;
using Microsoft.Extensions.Logging.Abstractions;
using SampleData.Entities;

namespace Infrastructure.Tests.Mapping;

public class AccountMappingProfileTests
{
	private readonly IMapper _mapper;

	public AccountMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<AccountMappingProfile>();
		}, NullLoggerFactory.Instance);

		configuration.AssertConfigurationIsValid();

		_mapper = configuration.CreateMapper();
	}

	[Fact]
	public void ShouldMapAccountToAccountEntity()
	{
		// Arrange
		Account expected = AccountGenerator.Generate();

		// Act
		AccountEntity mapped = _mapper.Map<AccountEntity>(expected);
		Account actual = _mapper.Map<Account>(mapped);

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}

	[Fact]
	public void ShouldMapAccountEntityToAccount()
	{
		// Arrange
		AccountEntity expected = AccountEntityGenerator.Generate();

		// Act
		Account mapped = _mapper.Map<Account>(expected);
		AccountEntity actual = _mapper.Map<AccountEntity>(mapped);

		// Assert
		actual.Should().BeEquivalentTo(expected);
	}
}
