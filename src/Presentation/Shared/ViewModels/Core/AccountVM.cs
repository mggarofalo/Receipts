namespace Shared.ViewModels.Core;

public class AccountVM : IEquatable<AccountVM>
{
	public Guid? Id { get; set; }
	public string? AccountCode { get; set; }
	public string? Name { get; set; }
	public bool IsActive { get; set; }

	public bool Equals(AccountVM? other)
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

		return Equals((AccountVM)obj);
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

	public static bool operator ==(AccountVM? left, AccountVM? right)
	{
		return Equals(left, right);
	}

	public static bool operator !=(AccountVM? left, AccountVM? right)
	{
		return !Equals(left, right);
	}
}
