using Common;
using Infrastructure.Interfaces;

namespace Infrastructure.Entities.Core;

public class TransactionEntity : ISoftDeletable, IOwnedBy<ReceiptEntity>
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public Guid AccountId { get; set; }
	public decimal Amount { get; set; }
	public Currency AmountCurrency { get; set; }
	public DateOnly Date { get; set; }
	public virtual ReceiptEntity? Receipt { get; set; }
	// Nav property preserves the Transaction→Account relationship name.
	// Type is temporarily CardEntity until Stage 2 introduces the logical Account layer.
	public virtual CardEntity? Account { get; set; }
	public DateTimeOffset? DeletedAt { get; set; }
	public string? DeletedByUserId { get; set; }
	public Guid? DeletedByApiKeyId { get; set; }
	public Guid? CascadeDeletedByParentId { get; set; }
}
