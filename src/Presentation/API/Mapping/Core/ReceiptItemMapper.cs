using API.Generated.Dtos;
using Common;
using Domain;
using Domain.Core;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Core;

[Mapper]
public partial class ReceiptItemMapper
{
	[MapProperty(nameof(ReceiptItem.UnitPrice.Amount), nameof(ReceiptItemResponse.UnitPrice))]
	[MapperIgnoreSource(nameof(ReceiptItem.TotalAmount))]
	[MapperIgnoreTarget(nameof(ReceiptItemResponse.AdditionalProperties))]
	public partial ReceiptItemResponse ToResponse(ReceiptItem source);

	public ReceiptItem ToDomain(CreateReceiptItemRequest source)
	{
		decimal quantity = (decimal)source.Quantity;
		decimal unitPrice = (decimal)source.UnitPrice;
		Money unitPriceMoney = new(unitPrice, Currency.USD);
		Money totalAmount = new(Math.Floor(quantity * unitPrice * 100) / 100, Currency.USD);

		return new ReceiptItem(
			Guid.Empty,
			source.ReceiptItemCode,
			source.Description,
			quantity,
			unitPriceMoney,
			totalAmount,
			source.Category,
			source.Subcategory
		);
	}

	public ReceiptItem ToDomain(UpdateReceiptItemRequest source)
	{
		decimal quantity = (decimal)source.Quantity;
		decimal unitPrice = (decimal)source.UnitPrice;
		Money unitPriceMoney = new(unitPrice, Currency.USD);
		Money totalAmount = new(Math.Floor(quantity * unitPrice * 100) / 100, Currency.USD);

		return new ReceiptItem(
			source.Id,
			source.ReceiptItemCode,
			source.Description,
			quantity,
			unitPriceMoney,
			totalAmount,
			source.Category,
			source.Subcategory
		);
	}

	private double MapDecimalToDouble(decimal value) => (double)value;
}
