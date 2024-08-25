namespace Domain.Core;

public class Account
{
	public Guid? Id { get; }
	public string AccountCode { get; }
	public string Name { get; }
	public bool IsActive { get; private set; }

	private Account(Guid? id, string accountCode, string name, bool isActive)
	{
		Id = id;
		AccountCode = accountCode;
		Name = name;
		IsActive = isActive;
	}

	public static Account Create(string accountCode, string name, bool isActive = true)
	{
		if (string.IsNullOrWhiteSpace(accountCode))
		{
			throw new ArgumentException("Account code cannot be empty", nameof(accountCode));
		}

		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Name cannot be empty", nameof(name));
		}

		return new Account(null, accountCode, name, isActive);
	}
}
