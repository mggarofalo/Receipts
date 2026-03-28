using Application.Interfaces;

namespace Application.Commands.Category.Delete;

public record DeleteCategoryCommand : ICommand<bool>
{
	public Guid Id { get; }

	public const string IdCannotBeEmpty = "Id cannot be empty.";

	public DeleteCategoryCommand(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmpty, nameof(id));
		}

		Id = id;
	}
}
