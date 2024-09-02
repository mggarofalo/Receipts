namespace Shared.ViewModels.Aggregates;

public class TripVM : IEquatable<TripVM>
{
	public required ReceiptWithItemsVM Receipt { get; set; }
	public required List<TransactionAccountVM> Transactions { get; set; }

	public bool Equals(TripVM? other)
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

		return Equals((TripVM)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Receipt);
		Transactions.ForEach(transaction => hash.Add(transaction));
		return hash.ToHashCode();
	}

	public static bool operator ==(TripVM? left, TripVM? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(TripVM? left, TripVM? right)
	{
		return !Equals(left, right);
	}
}