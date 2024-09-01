namespace Domain.Core;

public class Account
{
	public Guid? Id { get; }
	public string AccountCode { get; }
	public string Name { get; }
	public bool IsActive { get; private set; }

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
}