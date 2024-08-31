namespace Domain.Core;

public class Receipt
{
	public Guid? Id { get; }
	public string? Description { get; }
	public string Location { get; }
	public DateOnly Date { get; }
	public Money TaxAmount { get; }
	private readonly List<ReceiptItem> _items = [];
	public IReadOnlyList<ReceiptItem> Items => _items.AsReadOnly();
	private readonly List<Transaction> _transactions = [];
	public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

	public Receipt(Guid? id, string location, DateOnly date, Money taxAmount, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(location))
		{
			throw new ArgumentException("Location cannot be empty", nameof(location));
		}

		if (date.ToDateTime(new TimeOnly()) > DateTime.Today)
		{
			throw new ArgumentException("Date cannot be in the future", nameof(date));
		}

		Id = id;
		Location = location;
		Date = date;
		TaxAmount = taxAmount;
		Description = description;
	}

	public void AddTransaction(Transaction transaction)
	{
		_transactions.Add(transaction ?? throw new ArgumentNullException(nameof(transaction)));
	}

	public void AddItem(ReceiptItem item)
	{
		_items.Add(item ?? throw new ArgumentNullException(nameof(item)));
	}
}