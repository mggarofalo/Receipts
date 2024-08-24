namespace Domain;

public class Transaction
{
	public required Guid Id { get; set; } = Guid.NewGuid();
	public required Guid ReceiptId { get; set; }
	public required Guid AccountId { get; set; }
	public required decimal Amount { get; set; }
	public required DateTime Date { get; set; }
}
