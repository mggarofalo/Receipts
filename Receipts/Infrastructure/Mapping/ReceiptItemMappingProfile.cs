using AutoMapper;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;

namespace Infrastructure.Mapping;

public class ReceiptItemMappingProfile : Profile
{
	public ReceiptItemMappingProfile()
	{
		CreateMap<ReceiptItem, ReceiptItemEntity>()
			.ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
			.ForMember(dest => dest.UnitPriceCurrency, opt => opt.MapFrom(src => src.UnitPrice.Currency))
			.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount))
			.ForMember(dest => dest.TotalAmountCurrency, opt => opt.MapFrom(src => src.TotalAmount.Currency));

		CreateMap<ReceiptItemEntity, ReceiptItem>()
			.ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => new Money(src.UnitPrice, src.UnitPriceCurrency)))
			.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => new Money(src.TotalAmount, src.TotalAmountCurrency)));
	}
}