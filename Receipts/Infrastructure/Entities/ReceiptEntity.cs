namespace Infrastructure.Entities;

public class ReceiptEntity
{
	public required Guid Id { get; set; }
	public string? Description { get; set; }
	public required string Location { get; set; }
	public required DateTime Date { get; set; }
	public required decimal TaxAmount { get; set; }
	public required decimal TotalAmount { get; set; }
}
