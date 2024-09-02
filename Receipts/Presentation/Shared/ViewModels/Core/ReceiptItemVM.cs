namespace Shared.ViewModels.Core;

public class ReceiptItemVM
{
	public Guid? Id { get; set; }
	public Guid ReceiptId { get; set; }
	public required string ReceiptItemCode { get; set; }
	public required string Description { get; set; }
	public required decimal Quantity { get; set; }
	public required decimal UnitPrice { get; set; }
	public required decimal TotalAmount { get; set; }
	public required string Category { get; set; }
	public required string Subcategory { get; set; }
}
