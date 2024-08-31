using Application.Interfaces;

namespace Application.Queries.Receipt;

public record GetReceiptByIdQuery : IQuery<Domain.Core.Receipt?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "Receipt Id cannot be empty.";

	public GetReceiptByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}