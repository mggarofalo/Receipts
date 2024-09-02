using Shared.ViewModels.Core;

namespace Shared.ViewModels.Aggregates;

public class ReceiptWithItemsVM
{
	public required ReceiptVM Receipt { get; set; }
	public required List<ReceiptItemVM> Items { get; set; }
}