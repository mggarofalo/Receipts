using Domain.Core;

namespace Domain.Aggregates;

public class ReceiptWithItems : IEquatable<ReceiptWithItems>
{
	public required Receipt Receipt { get; set; }
	public required List<ReceiptItem> Items { get; set; }

	public bool Equals(ReceiptWithItems? other)
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
			Items.SequenceEqual(other.Items);
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

		return Equals((ReceiptWithItems)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Receipt);
		Items.ForEach(item => hash.Add(item));
		return hash.ToHashCode();
	}

	public static bool operator ==(ReceiptWithItems? left, ReceiptWithItems? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(ReceiptWithItems? left, ReceiptWithItems? right)
	{
		return !Equals(left, right);
	}
}