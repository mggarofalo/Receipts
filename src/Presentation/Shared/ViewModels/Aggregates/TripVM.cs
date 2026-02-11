namespace Shared.ViewModels.Aggregates;

public class TripVM
{
	public ReceiptWithItemsVM? Receipt { get; set; }
	public List<TransactionAccountVM>? Transactions { get; set; }
}