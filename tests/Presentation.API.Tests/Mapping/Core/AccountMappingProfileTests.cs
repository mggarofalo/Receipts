using API.Mapping.Core;
using AutoMapper;
using Domain.Core;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace Presentation.API.Tests.Mapping.Core;

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
	public void ShouldMapAccountToAccountVM()
	{
		// Arrange
		Account expected = AccountGenerator.Generate();

		// Act
		AccountVM mapped = _mapper.Map<AccountVM>(expected);
		Account actual = _mapper.Map<Account>(mapped);

		// Assert
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ShouldMapAccountVMToAccount()
	{
		// Arrange
		AccountVM expected = AccountVMGenerator.Generate();

		// Act
		Account mapped = _mapper.Map<Account>(expected);
		AccountVM actual = _mapper.Map<AccountVM>(mapped);

		// Assert
		Assert.Equal(expected, actual);
	}
}