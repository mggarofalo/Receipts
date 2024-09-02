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
	public required Currency UnitPriceCurrency { get; set; }
	public decimal TotalAmount { get; set; }
	public required Currency TotalAmountCurrency { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Subcategory { get; set; } = string.Empty;

	public bool Equals(ReceiptItemEntity? other)
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
			   ReceiptId.Equals(other.ReceiptId) &&
			   ReceiptItemCode == other.ReceiptItemCode &&
			   Description == other.Description &&
			   Quantity == other.Quantity &&
			   UnitPrice == other.UnitPrice &&
			   UnitPriceCurrency.Equals(other.UnitPriceCurrency) &&
			   TotalAmount == other.TotalAmount &&
			   TotalAmountCurrency.Equals(other.TotalAmountCurrency) &&
			   Category == other.Category &&
			   Subcategory == other.Subcategory;
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