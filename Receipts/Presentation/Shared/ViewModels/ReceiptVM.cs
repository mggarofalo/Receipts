namespace Shared.ViewModels;

public class ReceiptVM
{
	public Guid? Id { get; set; }
	public string? Description { get; set; }
	public string Location { get; set; } = string.Empty;
	public DateOnly Date { get; set; }
	public decimal TaxAmount { get; set; }

	public decimal Subtotal => Math.Floor(Items.Sum(i => i.TotalAmount) * 100) / 100;
	public decimal Total => Subtotal + TaxAmount;
	public required List<TransactionVM> Transactions { get; set; } = [];
	public required List<ReceiptItemVM> Items { get; set; } = [];
}
