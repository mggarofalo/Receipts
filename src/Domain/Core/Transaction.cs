namespace Domain.Core;

public class Transaction : IEquatable<Transaction>
{
	public Guid? Id { get; set; }
	public Money Amount { get; set; }
	public DateOnly Date { get; set; }

	public const string AmountMustBeNonZero = "Amount must be non-zero";
	public const string DateCannotBeInTheFuture = "Date cannot be in the future";

	public Transaction(Guid? id, Money amount, DateOnly date)
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

	public bool Equals(Transaction? other)
	{
		if (other is null)
		{
			return false;
		}

		return Amount == other.Amount &&
			   Date == other.Date;
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