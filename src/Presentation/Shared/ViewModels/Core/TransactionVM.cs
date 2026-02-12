namespace Shared.ViewModels.Core;

public class TransactionVM
{
	public Guid? Id { get; set; }
	public decimal? Amount { get; set; }
	public DateOnly? Date { get; set; }
}
