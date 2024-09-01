namespace Domain.Core;

public class ReceiptItem
{
	public Guid? Id { get; }
	public string ReceiptItemCode { get; }
	public string Description { get; }
	public decimal Quantity { get; }
	public Money UnitPrice { get; }
	public Money TotalAmount { get; }
	public string Category { get; }
	public string Subcategory { get; }

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
}