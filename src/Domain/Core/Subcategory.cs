namespace Domain.Core;

public class Subcategory
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public Guid CategoryId { get; set; }
	public string? Description { get; set; }
	public bool IsActive { get; set; }

	public const string NameCannotBeEmpty = "Name cannot be empty";
	public const string CategoryIdCannotBeEmpty = "Category ID cannot be empty";

	public Subcategory(Guid id, string name, Guid categoryId, string? description = null, bool isActive = true)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(NameCannotBeEmpty, nameof(name));
		}

		if (categoryId == Guid.Empty)
		{
			throw new ArgumentException(CategoryIdCannotBeEmpty, nameof(categoryId));
		}

		Id = id;
		Name = name;
		CategoryId = categoryId;
		Description = description;
		IsActive = isActive;
	}
}
