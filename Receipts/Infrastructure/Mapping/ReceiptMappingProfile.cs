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
			.ForMember(dest => dest.TaxAmountCurrency, opt => opt.MapFrom(src => src.TaxAmount.Currency))
			.ForMember(dest => dest.Transactions, opt => opt.Ignore())
			.ForMember(dest => dest.Items, opt => opt.Ignore());

		CreateMap<ReceiptEntity, Receipt>()
			.ForMember(dest => dest.TaxAmount, opt => opt.MapFrom(src => new Money(src.TaxAmount, src.TaxAmountCurrency)));
	}
}