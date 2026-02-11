using Common;

namespace Infrastructure.Entities.Core;

public class ReceiptItemEntity
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public string ReceiptItemCode { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public Currency UnitPriceCurrency { get; set; }
	public decimal TotalAmount { get; set; }
	public Currency TotalAmountCurrency { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Subcategory { get; set; } = string.Empty;
	public virtual ReceiptEntity? Receipt { get; set; }
}
