using Common;
using Infrastructure.Interfaces;

namespace Infrastructure.Entities.Core;

public class ReceiptEntity : ISoftDeletable
{
	public Guid Id { get; set; }
	public string Location { get; set; } = string.Empty;
	public DateOnly Date { get; set; }
	public decimal TaxAmount { get; set; }
	public Currency TaxAmountCurrency { get; set; }
	public string? OriginalImagePath { get; set; }
	public DateTimeOffset? DeletedAt { get; set; }
	public string? DeletedByUserId { get; set; }
	public Guid? DeletedByApiKeyId { get; set; }
	public Guid? CascadeDeletedByParentId { get; set; }
}
