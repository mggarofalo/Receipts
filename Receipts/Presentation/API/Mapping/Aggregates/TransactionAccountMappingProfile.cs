using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;

namespace API.Mapping.Aggregates;

public class TransactionAccountMappingProfile : Profile
{
	public TransactionAccountMappingProfile()
	{
		CreateMap<TransactionAccount, TransactionAccountVM>()
			.ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.Account))
			.ForMember(dest => dest.Transaction, opt => opt.MapFrom(src => src.Transaction));
	}
}