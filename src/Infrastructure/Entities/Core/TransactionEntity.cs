using Common;
using Infrastructure.Interfaces;

namespace Infrastructure.Entities.Core;

public class TransactionEntity : ISoftDeletable
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public Guid AccountId { get; set; }
	public decimal Amount { get; set; }
	public Currency AmountCurrency { get; set; }
	public DateOnly Date { get; set; }
	public virtual ReceiptEntity? Receipt { get; set; }
	public virtual AccountEntity? Account { get; set; }
	public DateTimeOffset? DeletedAt { get; set; }
	public string? DeletedByUserId { get; set; }
	public Guid? DeletedByApiKeyId { get; set; }
}
