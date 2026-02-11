namespace Shared.ViewModels.Core;

public class ReceiptItemVM
{
	public Guid? Id { get; set; }
	public string? ReceiptItemCode { get; set; }
	public string? Description { get; set; }
	public decimal? Quantity { get; set; }
	public decimal? UnitPrice { get; set; }
	public string? Category { get; set; }
	public string? Subcategory { get; set; }
}
