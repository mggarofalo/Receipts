namespace Infrastructure.Entities.Core;

public class ReceiptEntity
{
	public Guid Id { get; set; }
	public string? Description { get; set; }
	public string Location { get; set; } = string.Empty;
	public DateOnly Date { get; set; }
	public decimal TaxAmount { get; set; }
	public required string TaxAmountCurrency { get; set; }

	// Navigation properties
	public virtual ICollection<ReceiptItemEntity> ReceiptItems { get; set; } = [];
	public virtual ICollection<TransactionEntity> Transactions { get; set; } = [];
}