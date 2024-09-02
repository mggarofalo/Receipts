using Domain.Core;

namespace Domain.Aggregates;

public class ReceiptWithItems
{
	public required Receipt Receipt { get; set; }
	public required List<ReceiptItem> Items { get; set; }
}