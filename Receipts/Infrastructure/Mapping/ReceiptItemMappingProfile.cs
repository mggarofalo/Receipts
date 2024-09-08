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
			.ConstructUsing((src, context) =>
			{
				return new ReceiptItemEntity
				{
					Id = src.Id ?? Guid.Empty,
					ReceiptId = context.GetValueFromContext(nameof(ReceiptItemEntity.ReceiptId)),
					ReceiptItemCode = src.ReceiptItemCode,
					Description = src.Description,
					Quantity = src.Quantity,
					UnitPrice = src.UnitPrice.Amount,
					UnitPriceCurrency = src.UnitPrice.Currency,
					TotalAmount = src.TotalAmount.Amount,
					TotalAmountCurrency = src.TotalAmount.Currency
				};
			});

		CreateMap<ReceiptItemEntity, ReceiptItem>()
			.ConstructUsing((src, context) =>
			{
				return new ReceiptItem(
					src.Id == Guid.Empty ? null : src.Id,
					src.ReceiptItemCode,
					src.Description,
					src.Quantity,
					new Money(src.UnitPrice, src.UnitPriceCurrency),
					new Money(src.TotalAmount, src.TotalAmountCurrency),
					src.Category,
					src.Subcategory
				);
			});
	}
}