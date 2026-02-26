namespace Domain.Core;

public class ReceiptItem
{
	public Guid Id { get; set; }
	public Guid ReceiptId { get; set; }
	public string ReceiptItemCode { get; set; }
	public string Description { get; set; }
	public decimal Quantity { get; set; }
	public Money UnitPrice { get; set; }
	public Money TotalAmount { get; set; }
	// Category/Subcategory are stored as denormalized strings (not FK references).
	// This is intentional: values capture the historical categorization at time of entry,
	// while the Category/Subcategory tables serve as suggestion lists for the UI.
	public string Category { get; set; }
	public string Subcategory { get; set; }
	public string PricingMode { get; set; }

	public const string ReceiptItemCodeCannotBeEmpty = "Receipt item code cannot be empty";
	public const string DescriptionCannotBeEmpty = "Description cannot be empty";
	public const string QuantityMustBePositive = "Quantity must be positive";
	public const string CategoryCannotBeEmpty = "Category cannot be empty";
	public const string SubcategoryCannotBeEmpty = "Subcategory cannot be empty";
	public const string PricingModeInvalid = "Pricing mode must be 'quantity' or 'flat'";

	public ReceiptItem(Guid id, string receiptItemCode, string description, decimal quantity, Money unitPrice, Money totalAmount, string category, string subcategory, string pricingMode = "quantity")
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

		if (pricingMode is not "quantity" and not "flat")
		{
			throw new ArgumentException(PricingModeInvalid, nameof(pricingMode));
		}

		Id = id;
		ReceiptItemCode = receiptItemCode;
		Description = description;
		Quantity = quantity;
		UnitPrice = unitPrice;
		TotalAmount = totalAmount;
		Category = category;
		Subcategory = subcategory;
		PricingMode = pricingMode;
	}
}