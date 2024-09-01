using AutoMapper;
using Common;
using Domain;
using Domain.Core;
using Shared.ViewModels;

namespace API.Mapping;

public class ReceiptItemMappingProfile : Profile
{
	public ReceiptItemMappingProfile()
	{
		CreateMap<ReceiptItem, ReceiptItemVM>()
			.ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
			.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount));

		CreateMap<ReceiptItemVM, ReceiptItem>()
			.ConstructUsing(src => new(
				null,
				src.ReceiptId,
				src.ReceiptItemCode,
				src.Description,
				src.Quantity,
				new Money(src.UnitPrice, Currency.USD),
				new Money(src.TotalAmount, Currency.USD),
				src.Category,
				src.Subcategory
			));
	}
}