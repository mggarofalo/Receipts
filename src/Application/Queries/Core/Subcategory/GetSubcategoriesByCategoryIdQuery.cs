using Application.Interfaces;

namespace Application.Queries.Core.Subcategory;

public record GetSubcategoriesByCategoryIdQuery : IQuery<List<Domain.Core.Subcategory>>
{
	public Guid CategoryId { get; }
	public const string CategoryIdCannotBeEmptyExceptionMessage = "Category Id cannot be empty.";

	public GetSubcategoriesByCategoryIdQuery(Guid categoryId)
	{
		if (categoryId == Guid.Empty)
		{
			throw new ArgumentException(CategoryIdCannotBeEmptyExceptionMessage, nameof(categoryId));
		}

		CategoryId = categoryId;
	}
}
