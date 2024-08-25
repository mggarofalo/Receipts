namespace Shared.ViewModels;

public class ReceiptVM
{
	public Guid? Id { get; set; }
	public string? Description { get; set; }
	public string Location { get; set; } = string.Empty;
	public DateOnly Date { get; set; }
	public decimal TaxAmount { get; set; }

	public required List<TransactionVM> Transactions { get; set; } = [];
	public required List<ReceiptItemVM> Items { get; set; } = [];
}
