using Application.Interfaces;

namespace Application.Queries.Core.Adjustment;

public record GetAdjustmentByIdQuery : IQuery<Domain.Core.Adjustment?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "Adjustment Id cannot be empty.";

	public GetAdjustmentByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}
