using Application.Interfaces;

namespace Application.Queries.NormalizedDescription.GetById;

public record GetNormalizedDescriptionByIdQuery : IQuery<Domain.NormalizedDescriptions.NormalizedDescription?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "NormalizedDescription Id cannot be empty.";

	public GetNormalizedDescriptionByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}
