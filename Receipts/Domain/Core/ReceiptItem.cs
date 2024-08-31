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

	public ReceiptItem(Guid? id, string receiptItemCode, string description, decimal quantity, Money unitPrice, Money totalAmount, string category, string subcategory)
	{
		if (string.IsNullOrWhiteSpace(receiptItemCode))
		{
			throw new ArgumentException("Receipt item code cannot be empty", nameof(receiptItemCode));
		}

		if (string.IsNullOrWhiteSpace(description))
		{
			throw new ArgumentException("Description cannot be empty", nameof(description));
		}

		if (quantity <= 0)
		{
			throw new ArgumentException("Quantity must be positive", nameof(quantity));
		}

		if (string.IsNullOrWhiteSpace(category))
		{
			throw new ArgumentException("Category cannot be empty", nameof(category));
		}

		if (string.IsNullOrWhiteSpace(subcategory))
		{
			throw new ArgumentException("Subcategory cannot be empty", nameof(subcategory));
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