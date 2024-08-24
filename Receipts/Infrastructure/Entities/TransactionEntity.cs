namespace Infrastructure.Entities;

public class TransactionEntity
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public Guid AccountId { get; set; }
	public decimal Amount { get; set; }
	public DateTime Date { get; set; }

	public ReceiptEntity? Receipt { get; set; }
	public AccountEntity? Account { get; set; }
}
