using Common;

namespace Infrastructure.Entities.Core;

public class ReceiptEntity : IEquatable<ReceiptEntity>
{
	public Guid Id { get; set; }
	public string? Description { get; set; }
	public string Location { get; set; } = string.Empty;
	public DateOnly Date { get; set; }
	public decimal TaxAmount { get; set; }
	public Currency TaxAmountCurrency { get; set; }
	public virtual ICollection<ReceiptItemEntity>? Items { get; set; }
	public virtual ICollection<TransactionEntity>? Transactions { get; set; }

	public bool Equals(ReceiptEntity? other)
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

		return Equals((ReceiptEntity)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Id);
		hash.Add(Description);
		hash.Add(Location);
		hash.Add(Date);
		hash.Add(TaxAmount);
		hash.Add(TaxAmountCurrency);
		return hash.ToHashCode();
	}

	public static bool operator ==(ReceiptEntity? left, ReceiptEntity? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(ReceiptEntity? left, ReceiptEntity? right)
	{
		return !Equals(left, right);
	}
}