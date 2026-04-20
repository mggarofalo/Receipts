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
	[MapperIgnoreSource(nameof(ReceiptItem.PricingMode))]
	// NormalizedDescription FK and name are deliberately not exposed on the API response here —
	// RECEIPTS-583 will add them. Ignore them so Mapperly does not flag the unmapped members.
	[MapperIgnoreSource(nameof(ReceiptItem.NormalizedDescriptionId))]
	[MapperIgnoreSource(nameof(ReceiptItem.NormalizedDescriptionName))]
	[MapperIgnoreTarget(nameof(ReceiptItemResponse.AdditionalProperties))]
	[MapperIgnoreTarget(nameof(ReceiptItemResponse.PricingMode))]
	private partial ReceiptItemResponse ToResponsePartial(ReceiptItem source);

	public ReceiptItemResponse ToResponse(ReceiptItem source)
	{
		ReceiptItemResponse response = ToResponsePartial(source);
		response.PricingMode = source.PricingMode.ToString().ToLowerInvariant();
		return response;
	}

	public ReceiptItem ToDomain(CreateReceiptItemRequest source)
	{
		decimal quantity = (decimal)source.Quantity;
		decimal unitPrice = (decimal)source.UnitPrice;
		Money unitPriceMoney = new(unitPrice, Currency.USD);
		Money totalAmount = new(Math.Floor(quantity * unitPrice * 100) / 100, Currency.USD);

		PricingMode pricingMode = Enum.TryParse<PricingMode>(source.PricingMode, ignoreCase: true, out PricingMode mode)
			? mode : PricingMode.Quantity;

		return new ReceiptItem(
			Guid.Empty,
			source.ReceiptItemCode,
			source.Description,
			quantity,
			unitPriceMoney,
			totalAmount,
			source.Category,
			source.Subcategory,
			pricingMode
		);
	}

	public ReceiptItem ToDomain(UpdateReceiptItemRequest source)
	{
		decimal quantity = (decimal)source.Quantity;
		decimal unitPrice = (decimal)source.UnitPrice;
		Money unitPriceMoney = new(unitPrice, Currency.USD);
		Money totalAmount = new(Math.Floor(quantity * unitPrice * 100) / 100, Currency.USD);

		PricingMode pricingMode = Enum.TryParse<PricingMode>(source.PricingMode, ignoreCase: true, out PricingMode mode)
			? mode : PricingMode.Quantity;

		return new ReceiptItem(
			source.Id,
			source.ReceiptItemCode,
			source.Description,
			quantity,
			unitPriceMoney,
			totalAmount,
			source.Category,
			source.Subcategory,
			pricingMode
		);
	}

	private double MapDecimalToDouble(decimal value) => (double)value;
}
