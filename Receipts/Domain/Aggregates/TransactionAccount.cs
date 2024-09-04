using Domain.Core;

namespace Domain.Aggregates;

public class TransactionAccount : IEquatable<TransactionAccount>
{
	public required Transaction Transaction { get; set; }
	public required Account Account { get; set; }

	public bool Equals(TransactionAccount? other)
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

		return Equals((TransactionAccount)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Transaction);
		hash.Add(Account);
		return hash.ToHashCode();
	}

	public static bool operator ==(TransactionAccount? left, TransactionAccount? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(TransactionAccount? left, TransactionAccount? right)
	{
		return !Equals(left, right);
	}
}