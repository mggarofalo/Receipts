using AutoMapper;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public class ReceiptMappingProfile : Profile
{
	public ReceiptMappingProfile()
	{
		CreateMap<Receipt, ReceiptEntity>()
			.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => src.TaxAmount.Amount))
			.ForMember(dest => dest.TaxAmountCurrency, opt => opt.MapFrom(src => src.TaxAmount.Currency));

		CreateMap<ReceiptEntity, Receipt>()
			.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => new Money(src.TaxAmount, src.TaxAmountCurrency)));
	}
}