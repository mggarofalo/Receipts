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

		return Receipt == other.Receipt &&
			   Items.Count == other.Items.Count &&
			   Items.All(item => other.Items.Any(otherItem => item == otherItem));
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