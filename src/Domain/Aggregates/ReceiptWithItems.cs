using Domain.Core;

namespace Domain.Aggregates;

public class ReceiptWithItems
{
	public required Receipt Receipt { get; set; }
	public required List<ReceiptItem> Items { get; set; }
	public required List<Adjustment> Adjustments { get; set; }

	public Money Subtotal => Items.Aggregate(Money.Zero, (sum, item) => sum + item.TotalAmount);
	public Money AdjustmentTotal => Adjustments.Aggregate(Money.Zero, (sum, adj) => sum + adj.Amount);
	public Money ExpectedTotal => Subtotal + Receipt.TaxAmount + AdjustmentTotal;
}
