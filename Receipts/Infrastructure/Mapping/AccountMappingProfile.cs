using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public class AccountMappingProfile : Profile
{
	public AccountMappingProfile()
	{
		CreateMap<Account, AccountEntity>()
			.ForMember(dest => dest.Transactions, opt => opt.Ignore());

		CreateMap<AccountEntity, Account>();
	}
}
