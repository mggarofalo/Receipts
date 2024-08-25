namespace Shared.ViewModels;

public class TransactionVM
{
	public Guid? Id { get; set; }
	public required decimal Amount { get; set; }
	public required DateOnly Date { get; set; }

	public required AccountVM Account { get; set; }
	public Guid ReceiptId { get; set; }
}
