using API.Mapping.Core;
using AutoMapper;
using Domain.Core;
using SampleData.Domain.Core;
using SampleData.ViewModels.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Mapping.Core;

public class AccountMappingProfileTests
{
	private readonly IMapper _mapper;

	public AccountMappingProfileTests()
	{
		MapperConfiguration configuration = new(cfg =>
		{
			cfg.AddProfile<AccountMappingProfile>();
		});

		_mapper = configuration.CreateMapper();
		_mapper.ConfigurationProvider.AssertConfigurationIsValid();
	}

	[Fact]
	public void ShouldMapAccountToAccountVM()
	{
		// Arrange
		Account account = AccountGenerator.Generate();

		// Act
		AccountVM accountVM = _mapper.Map<AccountVM>(account);
		Account reverseMapped = _mapper.Map<Account>(accountVM);

		// Assert
		Assert.Equal(account, reverseMapped);
	}

	[Fact]
	public void ShouldMapAccountVMToAccount()
	{
		// Arrange
		AccountVM accountVM = AccountVMGenerator.Generate();

		// Act
		Account account = _mapper.Map<Account>(accountVM);
		AccountVM reverseMapped = _mapper.Map<AccountVM>(account);

		// Assert
		Assert.Equal(accountVM, reverseMapped);
	}
}