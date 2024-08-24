namespace Domain;

public class TransactionItem
{
	public required Guid Id { get; set; } = Guid.NewGuid();
	public required Guid TransactionId { get; set; }
	public required string ReceiptItemCode { get; set; }
	public required string Description { get; set; }
	public required decimal Quantity { get; set; }
	public required decimal UnitPrice { get; set; }
	public required string Category { get; set; }
	public required string Subcategory { get; set; }

	public decimal TotalAmount => Quantity * UnitPrice;
}
