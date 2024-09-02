using Common;

namespace Infrastructure.Entities.Core;

public class ReceiptEntity : IEquatable<ReceiptEntity>
{
	public Guid Id { get; set; }
	public string? Description { get; set; }
	public string Location { get; set; } = string.Empty;
	public DateOnly Date { get; set; }
	public decimal TaxAmount { get; set; }
	public required Currency TaxAmountCurrency { get; set; }

	public bool Equals(ReceiptEntity? other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return Id.Equals(other.Id) &&
			Description == other.Description &&
			Location == other.Location &&
			Date.Equals(other.Date) &&
			TaxAmount == other.TaxAmount &&
			TaxAmountCurrency.Equals(other.TaxAmountCurrency);
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