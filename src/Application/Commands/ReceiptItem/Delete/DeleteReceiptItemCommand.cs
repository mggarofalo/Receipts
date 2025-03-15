using Application.Interfaces;

namespace Application.Commands.ReceiptItem.Delete;

public record DeleteReceiptItemCommand : ICommand<bool>
{
	public IReadOnlyList<Guid> Ids { get; }

	public const string IdsListCannotBeEmpty = "Ids list cannot be empty.";

	public DeleteReceiptItemCommand(List<Guid> ids)
	{
		ArgumentNullException.ThrowIfNull(ids);

		if (ids.Count == 0)
		{
			throw new ArgumentException(IdsListCannotBeEmpty, nameof(ids));
		}

		Ids = ids.AsReadOnly();
	}
}
