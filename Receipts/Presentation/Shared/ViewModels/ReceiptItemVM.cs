namespace Shared.ViewModels;

public class ReceiptItemVM
{
	public Guid? Id { get; set; }
	public required string ReceiptItemCode { get; set; }
	public required string Description { get; set; }
	public required decimal Quantity { get; set; }
	public required decimal UnitPrice { get; set; }
	public required string Category { get; set; }
	public required string Subcategory { get; set; }

	public decimal TotalAmount => Math.Floor(Quantity * UnitPrice * 100) / 100;
	public Guid ReceiptId { get; set; }
}
