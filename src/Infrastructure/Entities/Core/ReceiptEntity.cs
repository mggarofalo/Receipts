using Common;

namespace Infrastructure.Entities.Core;

public class ReceiptEntity
{
	public Guid Id { get; set; }
	public string? Description { get; set; }
	public string Location { get; set; } = string.Empty;
	public DateOnly Date { get; set; }
	public decimal TaxAmount { get; set; }
	public Currency TaxAmountCurrency { get; set; }
}
