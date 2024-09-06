using AutoMapper;
using Domain.Aggregates;
using Shared.ViewModels.Aggregates;

namespace Shared.Mapping.Aggregates;

public class TransactionAccountMappingProfile : Profile
{
	public TransactionAccountMappingProfile()
	{
		CreateMap<TransactionAccount, TransactionAccountVM>().ReverseMap();
	}
}