using Common;
using Domain;
using Domain.Core;
using Riok.Mapperly.Abstractions;
using Shared.ViewModels.Core;

namespace API.Mapping.Core;

[Mapper]
public partial class ReceiptItemMapper
{
	[MapProperty(nameof(ReceiptItem.UnitPrice.Amount), nameof(ReceiptItemVM.UnitPrice))]
	[MapperIgnoreSource(nameof(ReceiptItem.TotalAmount))]
	public partial ReceiptItemVM ToViewModel(ReceiptItem source);

	private Guid MapId(Guid? id) => id ?? Guid.Empty;

	public ReceiptItem ToDomain(ReceiptItemVM source)
	{
		decimal quantity = source.Quantity ?? 0;
		decimal unitPrice = source.UnitPrice ?? 0;
		Money unitPriceMoney = new(unitPrice, Currency.USD);
		Money totalAmount = new(Math.Floor(quantity * unitPrice * 100) / 100, Currency.USD);

		return new ReceiptItem(
			source.Id ?? Guid.Empty,
			source.ReceiptItemCode ?? string.Empty,
			source.Description ?? string.Empty,
			quantity,
			unitPriceMoney,
			totalAmount,
			source.Category ?? string.Empty,
			source.Subcategory ?? string.Empty
		);
	}
}
