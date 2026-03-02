namespace Domain.Aggregates;

public class Trip
{
	public required ReceiptWithItems Receipt { get; set; }
	public required List<TransactionAccount> Transactions { get; set; }

	public Money TransactionTotal => Transactions.Aggregate(Money.Zero, (sum, ta) => sum + ta.Transaction.Amount);
}
