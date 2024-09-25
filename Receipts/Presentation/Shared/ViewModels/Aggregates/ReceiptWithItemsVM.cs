using Shared.ViewModels.Core;

namespace Shared.ViewModels.Aggregates;

public class ReceiptWithItemsVM : IEquatable<ReceiptWithItemsVM>
{
	public ReceiptVM? Receipt { get; set; }
	public List<ReceiptItemVM>? Items { get; set; }

	public bool Equals(ReceiptWithItemsVM? other)
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

		return Equals((ReceiptWithItemsVM)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Receipt);
		Items?.ForEach(item => hash.Add(item));
		return hash.ToHashCode();
	}

	public static bool operator ==(ReceiptWithItemsVM? left, ReceiptWithItemsVM? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(ReceiptWithItemsVM? left, ReceiptWithItemsVM? right)
	{
		return !Equals(left, right);
	}
}