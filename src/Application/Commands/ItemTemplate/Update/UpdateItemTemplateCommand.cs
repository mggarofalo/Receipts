using Application.Interfaces;

namespace Application.Commands.ItemTemplate.Update;

public record UpdateItemTemplateCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.ItemTemplate> ItemTemplates { get; }

	public const string ItemTemplatesListCannotBeEmpty = "Item templates list cannot be empty.";

	public UpdateItemTemplateCommand(List<Domain.Core.ItemTemplate> itemTemplates)
	{
		ArgumentNullException.ThrowIfNull(itemTemplates);

		if (itemTemplates.Count == 0)
		{
			throw new ArgumentException(ItemTemplatesListCannotBeEmpty, nameof(itemTemplates));
		}

		ItemTemplates = itemTemplates.AsReadOnly();
	}
}
