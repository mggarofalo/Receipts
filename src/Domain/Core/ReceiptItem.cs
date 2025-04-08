namespace Domain.Core;

public class ReceiptItem : IEquatable<ReceiptItem>
{
	public Guid? Id { get; set; }
	public string ReceiptItemCode { get; set; }
	public string Description { get; set; }
	public decimal Quantity { get; set; }
	public Money UnitPrice { get; set; }
	public Money TotalAmount { get; set; }
	public string Category { get; set; }
	public string Subcategory { get; set; }

	public const string ReceiptItemCodeCannotBeEmpty = "Receipt item code cannot be empty";
	public const string DescriptionCannotBeEmpty = "Description cannot be empty";
	public const string QuantityMustBePositive = "Quantity must be positive";
	public const string CategoryCannotBeEmpty = "Category cannot be empty";
	public const string SubcategoryCannotBeEmpty = "Subcategory cannot be empty";

	public ReceiptItem(Guid? id, string receiptItemCode, string description, decimal quantity, Money unitPrice, Money totalAmount, string category, string subcategory)
	{
		if (string.IsNullOrWhiteSpace(receiptItemCode))
		{
			throw new ArgumentException(ReceiptItemCodeCannotBeEmpty, nameof(receiptItemCode));
		}

		if (string.IsNullOrWhiteSpace(description))
		{
			throw new ArgumentException(DescriptionCannotBeEmpty, nameof(description));
		}

		if (quantity <= 0)
		{
			throw new ArgumentException(QuantityMustBePositive, nameof(quantity));
		}

		if (string.IsNullOrWhiteSpace(category))
		{
			throw new ArgumentException(CategoryCannotBeEmpty, nameof(category));
		}

		if (string.IsNullOrWhiteSpace(subcategory))
		{
			throw new ArgumentException(SubcategoryCannotBeEmpty, nameof(subcategory));
		}

		Id = id;
		ReceiptItemCode = receiptItemCode;
		Description = description;
		Quantity = quantity;
		UnitPrice = unitPrice;
		TotalAmount = totalAmount;
		Category = category;
		Subcategory = subcategory;
	}

	public bool Equals(ReceiptItem? other)
	{
		if (other is null)
		{
			return false;
		}

		return ReceiptItemCode == other.ReceiptItemCode &&
			   Description == other.Description &&
			   Quantity == other.Quantity &&
			   UnitPrice == other.UnitPrice &&
			   TotalAmount == other.TotalAmount &&
			   Category == other.Category &&
			   Subcategory == other.Subcategory;
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

		return Equals((ReceiptItem)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Id);
		hash.Add(ReceiptItemCode);
		hash.Add(Description);
		hash.Add(Quantity);
		hash.Add(UnitPrice);
		hash.Add(TotalAmount);
		hash.Add(Category);
		hash.Add(Subcategory);
		return hash.ToHashCode();
	}

	public static bool operator ==(ReceiptItem? left, ReceiptItem? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(ReceiptItem? left, ReceiptItem? right)
	{
		return !Equals(left, right);
	}
}