namespace Domain.Core;

public class Receipt
{
	public Guid Id { get; set; }
	public string? Description { get; set; }
	public string Location { get; set; }
	public DateOnly Date { get; set; }
	public Money TaxAmount { get; set; }

	public const string LocationCannotBeEmpty = "Location cannot be empty";
	public const string DateCannotBeInTheFuture = "Date cannot be in the future";

	public Receipt(Guid id, string location, DateOnly date, Money taxAmount, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(location))
		{
			throw new ArgumentException(LocationCannotBeEmpty, nameof(location));
		}

		if (date.ToDateTime(TimeOnly.MinValue) > DateTime.Today)
		{
			throw new ArgumentException(DateCannotBeInTheFuture, nameof(date));
		}

		Id = id;
		Location = location;
		Date = date;
		TaxAmount = taxAmount;
		Description = description;
	}
}