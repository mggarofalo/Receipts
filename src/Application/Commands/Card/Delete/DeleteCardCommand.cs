using Application.Interfaces;

namespace Application.Commands.Card.Delete;

public record DeleteCardCommand : ICommand<bool>
{
	public Guid Id { get; }

	public const string IdCannotBeEmpty = "Id cannot be empty.";

	public DeleteCardCommand(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmpty, nameof(id));
		}

		Id = id;
	}
}
