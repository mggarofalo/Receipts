namespace Infrastructure.Entities;

public class ReceiptItemEntity
{
	public Guid Id { get; set; }
	public Guid TransactionId { get; set; }
	public string ReceiptItemCode { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Subcategory { get; set; } = string.Empty;
	public decimal TotalAmount { get; set; }

	public TransactionEntity? Transaction { get; set; }
}
