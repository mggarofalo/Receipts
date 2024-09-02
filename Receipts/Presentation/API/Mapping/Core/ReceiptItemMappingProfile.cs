using AutoMapper;
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
			.ConstructUsing(src => new(
				src.Id,
				src.ReceiptId,
				src.ReceiptItemCode,
				src.Description,
				src.Quantity,
				new Money(src.UnitPrice),
				new Money(src.TotalAmount),
				src.Category,
				src.Subcategory
			));
	}
}