namespace Shared.ViewModels.Core;

public class ReceiptItemVM : IEquatable<ReceiptItemVM>
{
	public Guid? Id { get; set; }
	public Guid ReceiptId { get; set; }
	public required string ReceiptItemCode { get; set; }
	public required string Description { get; set; }
	public required decimal Quantity { get; set; }
	public required decimal UnitPrice { get; set; }
	public required decimal TotalAmount { get; set; } // TODO: Remove this property and calculate it in the mapper instead
	public required string Category { get; set; }
	public required string Subcategory { get; set; }

	public bool Equals(ReceiptItemVM? other)
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

		return Equals((ReceiptItemVM)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Id);
		hash.Add(ReceiptId);
		hash.Add(ReceiptItemCode);
		hash.Add(Description);
		hash.Add(Quantity);
		hash.Add(UnitPrice);
		hash.Add(TotalAmount);
		hash.Add(Category);
		hash.Add(Subcategory);
		return hash.ToHashCode();
	}

	public static bool operator ==(ReceiptItemVM? left, ReceiptItemVM? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(ReceiptItemVM? left, ReceiptItemVM? right)
	{
		return !Equals(left, right);
	}
}
