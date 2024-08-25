using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public class AccountMappingProfile : Profile
{
	public AccountMappingProfile()
	{
		CreateMap<Account, AccountEntity>()
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
			.ForMember(dest => dest.AccountCode, opt => opt.MapFrom(src => src.AccountCode))
			.ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
			.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

		CreateMap<AccountEntity, Account>()
			.ConstructUsing(src => new(
				null,
				src.AccountCode,
				src.Name,
				src.IsActive
			))
			.ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
	}
}