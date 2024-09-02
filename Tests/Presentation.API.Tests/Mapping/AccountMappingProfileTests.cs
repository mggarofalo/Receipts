using API.Mapping.Core;
using AutoMapper;
using Domain.Core;
using Shared.ViewModels.Core;

namespace Presentation.API.Tests.Mapping;

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
	}

	[Fact]
	public void ShouldMapAccountToAccountVM()
	{
		Account account = new(Guid.NewGuid(), "123456", "Test Account", true);
		AccountVM accountVM = _mapper.Map<AccountVM>(account);

		Assert.Equal(account.Id, accountVM.Id);
		Assert.Equal(account.AccountCode, accountVM.AccountCode);
		Assert.Equal(account.Name, accountVM.Name);
		Assert.Equal(account.IsActive, accountVM.IsActive);
	}

	[Fact]
	public void ShouldMapAccountVMToAccount()
	{
		AccountVM accountVM = new()
		{
			Id = Guid.NewGuid(),
			AccountCode = "123456",
			Name = "Test Account VM",
			IsActive = true
		};

		Account account = _mapper.Map<Account>(accountVM);

		Assert.Equal(accountVM.Id, account.Id);
		Assert.Equal(accountVM.AccountCode, account.AccountCode);
		Assert.Equal(accountVM.Name, account.Name);
		Assert.Equal(accountVM.IsActive, account.IsActive);

	}
}