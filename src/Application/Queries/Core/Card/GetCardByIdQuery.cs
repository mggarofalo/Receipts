using Application.Interfaces;

namespace Application.Queries.Core.Card;

public record GetCardByIdQuery : IQuery<Domain.Core.Card?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "Card Id cannot be empty.";

	public GetCardByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}
