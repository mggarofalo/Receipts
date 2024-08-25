namespace Domain.Core;

public class Transaction
{
	public Guid? Id { get; }
	public Guid ReceiptId { get; }
	public Guid AccountId { get; }
	public Money Amount { get; }
	public DateOnly Date { get; }

	private Transaction(Guid? id, Guid receiptId, Guid accountId, Money amount, DateOnly date)
	{
		Id = id;
		ReceiptId = receiptId;
		AccountId = accountId;
		Amount = amount;
		Date = date;
	}

	public static Transaction Create(Guid receiptId, Guid accountId, Money amount, DateOnly date)
	{
		if (receiptId == Guid.Empty)
		{
			throw new ArgumentException("Receipt ID cannot be empty", nameof(receiptId));
		}

		if (accountId == Guid.Empty)
		{
			throw new ArgumentException("Account ID cannot be empty", nameof(accountId));
		}

		if (amount.Amount == 0)
		{
			throw new ArgumentException("Amount must be non-zero", nameof(amount));
		}

		if (date.ToDateTime(new TimeOnly()) > DateTime.Today)
		{
			throw new ArgumentException("Date cannot be in the future", nameof(date));
		}

		return new Transaction(null, receiptId, accountId, amount, date);
	}
}