using AutoMapper;
using Common;
using Domain;
using Domain.Core;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

public class TransactionMappingProfile : Profile
{
	public TransactionMappingProfile()
	{
		CreateMap<Transaction, TransactionVM>()
			.ForMember(vm => vm.Amount, opt => opt.MapFrom(domain => domain.Amount.Amount));

		CreateMap<TransactionVM, Transaction>()
			.ForMember(domain => domain.Amount, opt => opt.MapFrom(vm => new Money(vm.Amount ?? 0, Currency.USD)));
	}
}