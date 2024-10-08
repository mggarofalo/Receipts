using AutoMapper;
using Domain.Core;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

public class AccountMappingProfile : Profile
{
	public AccountMappingProfile()
	{
		CreateMap<Account, AccountVM>().ReverseMap();
	}
}