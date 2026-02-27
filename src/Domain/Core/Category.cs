namespace Domain.Core;

public class Category
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string? Description { get; set; }

	public const string NameCannotBeEmpty = "Name cannot be empty";

	public Category(Guid id, string name, string? description = null)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(NameCannotBeEmpty, nameof(name));
		}

		Id = id;
		Name = name;
		Description = description;
	}
}
