namespace Infrastructure.Entities.Core;

public class AccountEntity : IEquatable<AccountEntity>
{
	public Guid Id { get; set; }
	public string AccountCode { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public bool IsActive { get; set; }
	public virtual ICollection<TransactionEntity>? Transactions { get; set; }

	public bool Equals(AccountEntity? other)
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

		return Equals((AccountEntity)obj);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(Id);
		hash.Add(AccountCode);
		hash.Add(Name);
		hash.Add(IsActive);
		return hash.ToHashCode();
	}

	public static bool operator ==(AccountEntity? left, AccountEntity? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(AccountEntity? left, AccountEntity? right)
	{
		return !Equals(left, right);
	}
}
