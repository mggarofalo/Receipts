namespace Domain.Core;

public class Transaction : IEquatable<Transaction>
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

		if (date.ToDateTime(TimeOnly.MinValue) > DateTime.Today)
		{
			throw new ArgumentException(DateCannotBeInTheFuture, nameof(date));
		}

		Id = id;
		ReceiptId = receiptId;
		AccountId = accountId;
		Amount = amount;
		Date = date;
	}

	public bool Equals(Transaction? other)
	{
		if (other is null)
		{
			return false;
		}

		return GetHashCode() == other.GetHashCode();
	}

	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((Transaction)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Id);
		hash.Add(ReceiptId);
		hash.Add(AccountId);
		hash.Add(Amount);
		hash.Add(Date);
		return hash.ToHashCode();
	}

	public static bool operator ==(Transaction? left, Transaction? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(Transaction? left, Transaction? right)
	{
		return !Equals(left, right);
	}
}