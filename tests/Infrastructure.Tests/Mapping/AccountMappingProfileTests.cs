using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using SampleData.Domain.Core;
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
		});

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
		Assert.Equal(expected, actual);
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
		Assert.Equal(expected, actual);
	}
}
