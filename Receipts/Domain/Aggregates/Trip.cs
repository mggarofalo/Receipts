namespace Domain.Aggregates;

public class Trip : IEquatable<Trip>
{
	public required ReceiptWithItems Receipt { get; set; }
	public required List<TransactionAccount> Transactions { get; set; }

	public bool Equals(Trip? other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return Receipt.Equals(other.Receipt) &&
			Transactions.SequenceEqual(other.Transactions);
	}

	public override bool Equals(object? obj)
	{
		if (obj is null)
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((Trip)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Receipt);
		Transactions.ForEach(transaction => hash.Add(transaction));
		return hash.ToHashCode();
	}

	public static bool operator ==(Trip? left, Trip? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(Trip? left, Trip? right)
	{
		return !Equals(left, right);
	}
}