using Application.Interfaces;

namespace Application.Queries.Core.Category;

public record GetCategoryByIdQuery : IQuery<Domain.Core.Category?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "Category Id cannot be empty.";

	public GetCategoryByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}
