using Application.Interfaces;

namespace Application.Queries.Core.ItemTemplate;

public record GetItemTemplateByIdQuery : IQuery<Domain.Core.ItemTemplate?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "Item template Id cannot be empty.";

	public GetItemTemplateByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}
