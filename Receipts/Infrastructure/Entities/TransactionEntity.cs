namespace Infrastructure.Entities;

public class TransactionEntity
{
	public required Guid Id { get; set; }
	public required Guid ReceiptId { get; set; }
	public required Guid AccountId { get; set; }
	public required decimal Amount { get; set; }
	public required DateTime Date { get; set; }

	public ReceiptEntity? Receipt { get; set; }
	public AccountEntity? Account { get; set; }
}
