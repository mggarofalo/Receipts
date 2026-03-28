using Application.Interfaces;

namespace Application.Commands.Subcategory.Delete;

public record DeleteSubcategoryCommand : ICommand<bool>
{
	public Guid Id { get; }

	public const string IdCannotBeEmpty = "Id cannot be empty.";

	public DeleteSubcategoryCommand(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmpty, nameof(id));
		}

		Id = id;
	}
}
