namespace Domain.Core;

public class Transaction
{
	public Guid Id { get; set; }
	public Money Amount { get; set; }
	public DateOnly Date { get; set; }

	public const string AmountMustBeNonZero = "Amount must be non-zero";
	public const string DateCannotBeInTheFuture = "Date cannot be in the future";

	public Transaction(Guid id, Money amount, DateOnly date)
	{
		if (amount.Amount == 0)
		{
			throw new ArgumentException(AmountMustBeNonZero, nameof(amount));
		}

		if (date.ToDateTime(TimeOnly.MinValue) > DateTime.Today)
		{
			throw new ArgumentException(DateCannotBeInTheFuture, nameof(date));
		}

		Id = id;
		Amount = amount;
		Date = date;
	}
}