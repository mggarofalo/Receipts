using Application.Interfaces;

namespace Application.Commands.Category.Update;

public record UpdateCategoryCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.Category> Categories { get; }

	public const string CategoriesListCannotBeEmpty = "Categories list cannot be empty.";

	public UpdateCategoryCommand(List<Domain.Core.Category> categories)
	{
		ArgumentNullException.ThrowIfNull(categories);

		if (categories.Count == 0)
		{
			throw new ArgumentException(CategoriesListCannotBeEmpty, nameof(categories));
		}

		Categories = categories.AsReadOnly();
	}
}
