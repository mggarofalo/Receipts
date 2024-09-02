using Shared.ViewModels.Core;

namespace Shared.ViewModels.Aggregates;

public class TransactionAccountVM : IEquatable<TransactionAccountVM>
{
	public required TransactionVM Transaction { get; set; }
	public required AccountVM Account { get; set; }

	public bool Equals(TransactionAccountVM? other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return Transaction.Equals(other.Transaction) &&
			Account.Equals(other.Account);
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

		return Equals((TransactionAccountVM)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Transaction);
		hash.Add(Account);
		return hash.ToHashCode();
	}

	public static bool operator ==(TransactionAccountVM? left, TransactionAccountVM? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(TransactionAccountVM? left, TransactionAccountVM? right)
	{
		return !Equals(left, right);
	}
}