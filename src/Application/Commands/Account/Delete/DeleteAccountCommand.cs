using Application.Interfaces;

namespace Application.Commands.Account.Delete;

public record DeleteAccountCommand : ICommand<bool>
{
	public IReadOnlyList<Guid> Ids { get; }

	public const string IdsListCannotBeEmpty = "Ids list cannot be empty.";

	public DeleteAccountCommand(List<Guid> ids)
	{
		ArgumentNullException.ThrowIfNull(ids);

		if (ids.Count == 0)
		{
			throw new ArgumentException(IdsListCannotBeEmpty, nameof(ids));
		}

		Ids = ids.AsReadOnly();
	}
}
