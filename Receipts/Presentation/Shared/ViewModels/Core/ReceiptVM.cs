namespace Shared.ViewModels.Core;

public class ReceiptVM : IEquatable<ReceiptVM>
{
	public Guid? Id { get; set; }
	public string? Description { get; set; }
	public string Location { get; set; } = string.Empty;
	public DateOnly Date { get; set; }
	public decimal TaxAmount { get; set; }

	public bool Equals(ReceiptVM? other)
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

		return Equals((ReceiptVM)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Id);
		hash.Add(Description);
		hash.Add(Location);
		hash.Add(Date);
		hash.Add(TaxAmount);
		return hash.ToHashCode();
	}

	public static bool operator ==(ReceiptVM? left, ReceiptVM? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(ReceiptVM? left, ReceiptVM? right)
	{
		return !Equals(left, right);
	}
}
