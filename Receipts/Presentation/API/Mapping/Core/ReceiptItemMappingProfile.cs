using AutoMapper;
using Common;
using Domain;
using Domain.Core;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

public class ReceiptItemMappingProfile : Profile
{
	public ReceiptItemMappingProfile()
	{
		CreateMap<ReceiptItem, ReceiptItemVM>()
			.ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
			.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount));

		CreateMap<ReceiptItemVM, ReceiptItem>()
			.ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => new Money(src.UnitPrice, Currency.USD)))
			.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => new Money(src.TotalAmount, Currency.USD)));
	}
}