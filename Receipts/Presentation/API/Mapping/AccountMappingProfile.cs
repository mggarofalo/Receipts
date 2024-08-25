using AutoMapper;
using Domain.Core;
using Shared.ViewModels;

namespace API.Mapping;

public class AccountMappingProfile : Profile
{
	public AccountMappingProfile()
	{
		CreateMap<Account, AccountVM>().ReverseMap();
	}
}