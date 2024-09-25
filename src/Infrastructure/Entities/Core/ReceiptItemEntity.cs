using Common;

namespace Infrastructure.Entities.Core;

public class ReceiptItemEntity : IEquatable<ReceiptItemEntity>
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public string ReceiptItemCode { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public decimal Quantity { get; set; }
	public decimal UnitPrice { get; set; }
	public Currency UnitPriceCurrency { get; set; }
	public decimal TotalAmount { get; set; }
	public Currency TotalAmountCurrency { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Subcategory { get; set; } = string.Empty;
	public virtual ReceiptEntity? Receipt { get; set; }

	public bool Equals(ReceiptItemEntity? other)
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

		return Equals((ReceiptItemEntity)obj);
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
		hash.Add(UnitPriceCurrency);
		hash.Add(TotalAmount);
		hash.Add(TotalAmountCurrency);
		hash.Add(Category);
		hash.Add(Subcategory);
		return hash.ToHashCode();
	}

	public static bool operator ==(ReceiptItemEntity? left, ReceiptItemEntity? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(ReceiptItemEntity? left, ReceiptItemEntity? right)
	{
		return !Equals(left, right);
	}
}