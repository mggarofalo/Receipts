namespace Domain.Core;

public class Receipt : IEquatable<Receipt>
{
	public Guid? Id { get; }
	public string? Description { get; }
	public string Location { get; }
	public DateOnly Date { get; }
	public Money TaxAmount { get; }

	public const string LocationCannotBeEmpty = "Location cannot be empty";
	public const string DateCannotBeInTheFuture = "Date cannot be in the future";

	public Receipt(Guid? id, string location, DateOnly date, Money taxAmount, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(location))
		{
			throw new ArgumentException(LocationCannotBeEmpty, nameof(location));
		}

		if (date.ToDateTime(new TimeOnly()) > DateTime.Today)
		{
			throw new ArgumentException(DateCannotBeInTheFuture, nameof(date));
		}

		Id = id;
		Location = location;
		Date = date;
		TaxAmount = taxAmount;
		Description = description;
	}

	public bool Equals(Receipt? other)
	{
		if (other is null)
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return Id == other.Id &&
			Description == other.Description &&
			Location == other.Location &&
			Date.Equals(other.Date) &&
			TaxAmount.Equals(other.TaxAmount);
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

		return Equals((Receipt)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Id);
		hash.Add(Description);
		hash.Add(Location);
		hash.Add(Date);
		hash.Add(TaxAmount);
		return hash.ToHashCode();
	}

	public static bool operator ==(Receipt? left, Receipt? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(Receipt? left, Receipt? right)
	{
		return !Equals(left, right);
	}
}