using Application.Interfaces;

namespace Application.Commands.ItemTemplate.Create;

public record CreateItemTemplateCommand : ICommand<List<Domain.Core.ItemTemplate>>
{
	public IReadOnlyList<Domain.Core.ItemTemplate> ItemTemplates { get; }

	public const string ItemTemplatesListCannotBeEmpty = "Item templates list cannot be empty.";

	public CreateItemTemplateCommand(List<Domain.Core.ItemTemplate> itemTemplates)
	{
		ArgumentNullException.ThrowIfNull(itemTemplates);

		if (itemTemplates.Count == 0)
		{
			throw new ArgumentException(ItemTemplatesListCannotBeEmpty, nameof(itemTemplates));
		}

		ItemTemplates = itemTemplates.AsReadOnly();
	}
}
