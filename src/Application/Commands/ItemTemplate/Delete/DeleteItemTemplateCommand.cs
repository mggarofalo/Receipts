using Application.Interfaces;

namespace Application.Commands.ItemTemplate.Delete;

public record DeleteItemTemplateCommand : ICommand<bool>
{
	public IReadOnlyList<Guid> Ids { get; }

	public const string IdsListCannotBeEmpty = "Ids list cannot be empty.";

	public DeleteItemTemplateCommand(List<Guid> ids)
	{
		ArgumentNullException.ThrowIfNull(ids);

		if (ids.Count == 0)
		{
			throw new ArgumentException(IdsListCannotBeEmpty, nameof(ids));
		}

		Ids = ids.AsReadOnly();
	}
}
