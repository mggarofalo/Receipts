using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public class AccountMappingProfile : Profile
{
	public AccountMappingProfile()
	{
		CreateMap<Account, AccountEntity>();

		CreateMap<AccountEntity, Account>()
			.ConstructUsing(src => new(null, src.AccountCode, src.Name, src.IsActive));
	}
}