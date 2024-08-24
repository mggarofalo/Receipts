namespace Infrastructure.Entities;

public class TransactionItemEntity
{
	public required Guid Id { get; set; }
	public required Guid TransactionId { get; set; }
	public string? Description { get; set; }
	public required decimal Quantity { get; set; }
	public required decimal UnitPrice { get; set; }
	public required decimal TotalAmount { get; set; }

	public TransactionEntity? Transaction { get; set; }
}
