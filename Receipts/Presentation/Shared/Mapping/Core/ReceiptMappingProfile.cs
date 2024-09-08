using AutoMapper;
using Common;
using Domain;
using Domain.Core;
using Shared.ViewModels.Core;

namespace Shared.Mapping.Core;

public class ReceiptMappingProfile : Profile
{
	public ReceiptMappingProfile()
	{
		CreateMap<Receipt, ReceiptVM>()
			.ForMember(vm => vm.TaxAmount, opt => opt.MapFrom(domain => domain.TaxAmount.Amount));

		CreateMap<ReceiptVM, Receipt>()
			.ForPath(domain => domain.TaxAmount, opt => opt.MapFrom(vm => new Money(vm.TaxAmount, Currency.USD)));
	}
}