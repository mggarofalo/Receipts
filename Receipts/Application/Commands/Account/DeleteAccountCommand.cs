using Application.Interfaces;

namespace Application.Commands.Account;

public record DeleteAccountCommand : ICommand<bool>
{
	public IReadOnlyList<Guid> Ids { get; }
	public const string IdsCannotBeEmptyExceptionMessage = "Ids list cannot be empty.";

	public DeleteAccountCommand(List<Guid> ids)
	{
		ArgumentNullException.ThrowIfNull(ids);

		if (ids.Count == 0)
		{
			throw new ArgumentException(IdsCannotBeEmptyExceptionMessage, nameof(ids));
		}

		Ids = ids.AsReadOnly();
	}
}
