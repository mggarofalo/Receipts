namespace Domain.Aggregates;

public class Trip
{
	public const string BalanceEquationViolation = "Balance equation violated: expected total ({0:C}) does not equal transaction total ({1:C})";
	public const string TransactionDateBeforeReceiptDate = "Transaction on {0} dated {1} is before receipt date {2}";

	public required ReceiptWithItems Receipt { get; set; }
	public required List<TransactionAccount> Transactions { get; set; }

	public Money TransactionTotal => Transactions.Aggregate(Money.Zero, (sum, ta) => sum + ta.Transaction.Amount);

	public List<string> Validate()
	{
		List<string> errors = [];

		if (Transactions.Count > 0 && Math.Abs(Receipt.ExpectedTotal.Amount - TransactionTotal.Amount) > 0.01m)
		{
			errors.Add(string.Format(BalanceEquationViolation, Receipt.ExpectedTotal.Amount, TransactionTotal.Amount));
		}

		return errors;
	}

	public List<ValidationWarning> GetWarnings()
	{
		List<ValidationWarning> warnings = Receipt.GetWarnings();

		DateOnly receiptDate = Receipt.Receipt.Date;
		foreach (TransactionAccount ta in Transactions)
		{
			if (ta.Transaction.Date < receiptDate)
			{
				warnings.Add(new ValidationWarning(
					"Transaction.Date",
					string.Format(TransactionDateBeforeReceiptDate,
						ta.Transaction.Id, ta.Transaction.Date, receiptDate),
					ValidationWarningSeverity.Warning));
			}
		}

		return warnings;
	}
}
