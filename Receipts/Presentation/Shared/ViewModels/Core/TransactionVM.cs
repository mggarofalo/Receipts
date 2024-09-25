namespace Shared.ViewModels.Core;

public class TransactionVM : IEquatable<TransactionVM>
{
	public Guid? Id { get; set; }
	public decimal? Amount { get; set; }
	public DateOnly? Date { get; set; }

	public bool Equals(TransactionVM? other)
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

		return Equals((TransactionVM)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Id);
		hash.Add(Amount);
		hash.Add(Date);
		return hash.ToHashCode();
	}

	public static bool operator ==(TransactionVM? left, TransactionVM? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(TransactionVM? left, TransactionVM? right)
	{
		return !Equals(left, right);
	}
}
