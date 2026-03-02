namespace Domain.Aggregates;

public class Trip
{
	public const string BalanceEquationViolation = "Balance equation violated: expected total ({0:C}) does not equal transaction total ({1:C})";

	public required ReceiptWithItems Receipt { get; set; }
	public required List<TransactionAccount> Transactions { get; set; }

	public Money TransactionTotal => Transactions.Aggregate(Money.Zero, (sum, ta) => sum + ta.Transaction.Amount);

	public List<string> Validate()
	{
		List<string> errors = [];

		if (Transactions.Count > 0 && Receipt.ExpectedTotal.Amount != TransactionTotal.Amount)
		{
			errors.Add(string.Format(BalanceEquationViolation, Receipt.ExpectedTotal.Amount, TransactionTotal.Amount));
		}

		return errors;
	}
}
