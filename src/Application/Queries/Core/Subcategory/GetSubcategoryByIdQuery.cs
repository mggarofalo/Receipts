using Application.Interfaces;

namespace Application.Queries.Core.Subcategory;

public record GetSubcategoryByIdQuery : IQuery<Domain.Core.Subcategory?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "Subcategory Id cannot be empty.";

	public GetSubcategoryByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}
