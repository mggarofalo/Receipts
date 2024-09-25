namespace Shared.ViewModels.Aggregates;

public class TripVM : IEquatable<TripVM>
{
	public ReceiptWithItemsVM? Receipt { get; set; }
	public List<TransactionAccountVM>? Transactions { get; set; }

	public bool Equals(TripVM? other)
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

		return Equals((TripVM)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Receipt);
		Transactions?.ForEach(transaction => hash.Add(transaction));
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