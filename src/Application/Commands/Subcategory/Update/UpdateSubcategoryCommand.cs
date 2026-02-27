using Application.Interfaces;

namespace Application.Commands.Subcategory.Update;

public record UpdateSubcategoryCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.Subcategory> Subcategories { get; }

	public const string SubcategoriesListCannotBeEmpty = "Subcategories list cannot be empty.";

	public UpdateSubcategoryCommand(List<Domain.Core.Subcategory> subcategories)
	{
		ArgumentNullException.ThrowIfNull(subcategories);

		if (subcategories.Count == 0)
		{
			throw new ArgumentException(SubcategoriesListCannotBeEmpty, nameof(subcategories));
		}

		Subcategories = subcategories.AsReadOnly();
	}
}
