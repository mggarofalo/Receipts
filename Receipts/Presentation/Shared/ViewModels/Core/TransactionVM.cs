namespace Shared.ViewModels.Core;

public class TransactionVM
{
	public Guid? Id { get; set; }
	public Guid ReceiptId { get; set; }
	public Guid AccountId { get; set; }
	public required decimal Amount { get; set; }
	public required DateOnly Date { get; set; }
}
