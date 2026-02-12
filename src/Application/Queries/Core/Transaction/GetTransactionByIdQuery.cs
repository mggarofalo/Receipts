using Application.Interfaces;

namespace Application.Queries.Core.Transaction;

public record GetTransactionByIdQuery : IQuery<Domain.Core.Transaction?>
{
	public Guid Id { get; }
	public const string IdCannotBeEmptyExceptionMessage = "Transaction Id cannot be empty.";

	public GetTransactionByIdQuery(Guid id)
	{
		if (id == Guid.Empty)
		{
			throw new ArgumentException(IdCannotBeEmptyExceptionMessage, nameof(id));
		}

		Id = id;
	}
}
