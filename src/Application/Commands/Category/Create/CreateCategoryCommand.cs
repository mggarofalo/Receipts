using Application.Interfaces;

namespace Application.Commands.Category.Create;

public record CreateCategoryCommand : ICommand<List<Domain.Core.Category>>
{
	public IReadOnlyList<Domain.Core.Category> Categories { get; }

	public const string CategoriesListCannotBeEmpty = "Categories list cannot be empty.";

	public CreateCategoryCommand(List<Domain.Core.Category> categories)
	{
		ArgumentNullException.ThrowIfNull(categories);

		if (categories.Count == 0)
		{
			throw new ArgumentException(CategoriesListCannotBeEmpty, nameof(categories));
		}

		Categories = categories.AsReadOnly();
	}
}
