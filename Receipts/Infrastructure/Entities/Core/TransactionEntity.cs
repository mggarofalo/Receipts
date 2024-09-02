using Common;

namespace Infrastructure.Entities.Core;

public class TransactionEntity
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public Guid AccountId { get; set; }
	public decimal Amount { get; set; }
	public required Currency AmountCurrency { get; set; }
	public DateOnly Date { get; set; }
}