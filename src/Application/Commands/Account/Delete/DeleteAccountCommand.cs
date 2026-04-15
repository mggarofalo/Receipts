using Application.Interfaces;

namespace Application.Commands.Account.Delete;

public record DeleteAccountCommand : ICommand<bool>
{
	public Guid Id { get; }

	public const string IdCannotBeEmptyExceptionMessage = "Account Id cannot be empty.";

	public DeleteAccountCommand(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}
