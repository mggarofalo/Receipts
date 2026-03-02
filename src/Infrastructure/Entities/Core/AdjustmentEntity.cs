using Common;
using Infrastructure.Interfaces;

namespace Infrastructure.Entities.Core;

public class AdjustmentEntity : ISoftDeletable
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public AdjustmentType Type { get; set; }
	public decimal Amount { get; set; }
	public Currency AmountCurrency { get; set; }
	public string? Description { get; set; }
	public virtual ReceiptEntity? Receipt { get; set; }
	public DateTimeOffset? DeletedAt { get; set; }
	public string? DeletedByUserId { get; set; }
	public Guid? DeletedByApiKeyId { get; set; }
}
