namespace Domain.Core;

public class Account : IEquatable<Account>
{
	public Guid? Id { get; set; }
	public string AccountCode { get; set; }
	public string Name { get; set; }
	public bool IsActive { get; set; }

	public const string AccountCodeCannotBeEmpty = "Account code cannot be empty";
	public const string NameCannotBeEmpty = "Name cannot be empty";

	public Account(Guid? id, string accountCode, string name, bool isActive = true)
	{
		if (string.IsNullOrWhiteSpace(accountCode))
		{
			throw new ArgumentException(AccountCodeCannotBeEmpty, nameof(accountCode));
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(NameCannotBeEmpty, nameof(name));
		}

		Id = id;
		AccountCode = accountCode;
		Name = name;
		IsActive = isActive;
	}

	public bool Equals(Account? other)
	{
		if (other is null)
		{
			return false;
		}

		return AccountCode == other.AccountCode &&
			   Name == other.Name &&
			   IsActive == other.IsActive;
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

		return Equals((Account)obj);
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

	public static bool operator ==(Account? left, Account? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(Account? left, Account? right)
	{
		return !Equals(left, right);
	}
}