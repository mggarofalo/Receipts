using AutoMapper;
using Common;
using Domain;
using Domain.Core;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

public class ReceiptMappingProfile : Profile
{
	public ReceiptMappingProfile()
	{
		CreateMap<Receipt, ReceiptVM>()
			.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount));

		CreateMap<ReceiptVM, Receipt>()
			.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => new Money(src.TaxAmount, Currency.USD)));
	}
}