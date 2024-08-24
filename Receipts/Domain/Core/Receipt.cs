namespace Domain.Core;

public class Receipt
{
	public Guid Id { get; }
	public string? Description { get; }
	public string Location { get; }
	public DateTime Date { get; }
	public Money TaxAmount { get; }
	public Money TotalAmount { get; }
	private List<ReceiptItem> _items = [];
	public IReadOnlyList<ReceiptItem> Items => _items.AsReadOnly();
	private List<Transaction> _transactions = [];
	public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

	private Receipt(Guid id, string location, DateTime date, Money taxAmount, Money totalAmount, string? description = null)
	{
		Id = id;
		Location = location;
		Date = date;
		TaxAmount = taxAmount;
		TotalAmount = totalAmount;
		Description = description;
	}

	public static Receipt Create(string location, DateTime date, Money taxAmount, Money totalAmount, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(location))
		{
			throw new ArgumentException("Location cannot be empty", nameof(location));
		}

		if (date > DateTime.UtcNow)
		{
			throw new ArgumentException("Date cannot be in the future", nameof(date));
		}

		if (taxAmount.Amount < 0)
		{
			throw new ArgumentException("Tax amount cannot be negative", nameof(taxAmount));
		}

		if (totalAmount.Amount <= 0)
		{
			throw new ArgumentException("Total amount must be positive", nameof(totalAmount));
		}

		return new Receipt(Guid.NewGuid(), location, date, taxAmount, totalAmount, description);
	}

	public void AddTransaction(Transaction transaction)
	{
		if (transaction != null)
		{
			_transactions.Add(transaction);
		}
		else
		{
			throw new ArgumentNullException(nameof(transaction));
		}
	}

	public void AddItem(ReceiptItem item)
	{
		if (item != null)
		{
			_items.Add(item);
		}
		else
		{
			throw new ArgumentNullException(nameof(item));
		}
	}

	public Money CalculateSubtotal()
	{
		return new Money(TotalAmount.Amount - TaxAmount.Amount);
	}
}
