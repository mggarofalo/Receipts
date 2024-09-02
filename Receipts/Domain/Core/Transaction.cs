namespace Domain.Core;

public class Transaction
{
	public Guid? Id { get; }
	public Guid ReceiptId { get; }
	public Guid AccountId { get; }
	public Money Amount { get; }
	public DateOnly Date { get; }

	public const string ReceiptIdCannotBeEmpty = "Receipt ID cannot be empty";
	public const string AmountMustBeNonZero = "Amount must be non-zero";
	public const string DateCannotBeInTheFuture = "Date cannot be in the future";
	public const string AccountIdCannotBeEmpty = "Account ID cannot be empty";

	public Transaction(Guid? id, Guid receiptId, Guid accountId, Money amount, DateOnly date)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException(ReceiptIdCannotBeEmpty, nameof(receiptId));
		}

		if (accountId == Guid.Empty)
		{
			throw new ArgumentException(AccountIdCannotBeEmpty, nameof(accountId));
		}

		if (amount.Amount == 0)
		{
			throw new ArgumentException(AmountMustBeNonZero, nameof(amount));
		}

		if (date.ToDateTime(new TimeOnly()) > DateTime.Today)
		{
			throw new ArgumentException(DateCannotBeInTheFuture, nameof(date));
		}

		Id = id;
		ReceiptId = receiptId;
		AccountId = accountId;
		Amount = amount;
		Date = date;
	}
}