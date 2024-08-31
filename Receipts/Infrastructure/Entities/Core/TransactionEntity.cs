namespace Infrastructure.Entities.Core;

public class TransactionEntity
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public Guid AccountId { get; set; }
	public decimal Amount { get; set; }
	public required string AmountCurrency { get; set; }
	public DateOnly Date { get; set; }

	// Navigation properties
	public virtual ReceiptEntity? Receipt { get; set; }
	public virtual AccountEntity? Account { get; set; }
}