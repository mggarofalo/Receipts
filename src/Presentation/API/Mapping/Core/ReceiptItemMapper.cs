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
	[MapProperty(nameof(ReceiptItem.TotalAmount.Amount), nameof(ReceiptItemVM.TotalAmount))]
	public partial ReceiptItemVM ToViewModel(ReceiptItem source);

	private Money MapUnitPrice(decimal? unitPrice) => new(unitPrice ?? 0, Currency.USD);
	private Money MapTotalAmount(decimal? totalAmount) => new(totalAmount ?? 0, Currency.USD);
	private Guid MapId(Guid? id) => id ?? Guid.Empty;

	public partial ReceiptItem ToDomain(ReceiptItemVM source);
}
