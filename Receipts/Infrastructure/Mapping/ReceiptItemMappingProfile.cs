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
			.ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount));

		CreateMap<ReceiptItemEntity, ReceiptItem>()
			.ConstructUsing(src => new(null, src.ReceiptItemCode, src.Description, src.Quantity, new Money(src.UnitPrice, "USD"), src.Category, src.Subcategory));
	}
}