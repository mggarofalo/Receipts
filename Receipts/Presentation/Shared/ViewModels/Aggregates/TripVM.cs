namespace Shared.ViewModels.Aggregates;

public class TripVM
{
	public required ReceiptWithItemsVM Receipt { get; set; }
	public required List<TransactionAccountVM> Transactions { get; set; }
}