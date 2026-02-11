using Shared.ViewModels.Core;

namespace Shared.ViewModels.Aggregates;

public class ReceiptWithItemsVM
{
	public ReceiptVM? Receipt { get; set; }
	public List<ReceiptItemVM>? Items { get; set; }
}