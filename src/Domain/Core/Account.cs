namespace Domain.Core;

public class Account
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public bool IsActive { get; set; }

	public const string NameCannotBeEmpty = "Name cannot be empty";

	public Account(Guid id, string name, bool isActive = true)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(NameCannotBeEmpty, nameof(name));
		}

		Id = id;
		Name = name;
		IsActive = isActive;
	}
}
