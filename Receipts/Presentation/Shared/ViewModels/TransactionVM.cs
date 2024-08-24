namespace Shared.ViewModels;

public class TransactionVM
{
	public Guid? Id { get; set; }
	public required decimal Amount { get; set; }
	public required DateTime Date { get; set; }

	public required AccountVM Account { get; set; }
}
