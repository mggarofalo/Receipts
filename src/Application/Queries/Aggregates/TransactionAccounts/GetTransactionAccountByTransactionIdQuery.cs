using Application.Interfaces;

namespace Application.Queries.Aggregates.TransactionAccounts;

public record GetTransactionAccountByTransactionIdQuery : IQuery<Domain.Aggregates.TransactionAccount?>
{
	public Guid TransactionId { get; }
	public const string TransactionIdCannotBeEmptyExceptionMessage = "Transaction Id cannot be empty.";

	public GetTransactionAccountByTransactionIdQuery(Guid transactionId)
	{
		if (transactionId == Guid.Empty)
		{
			throw new ArgumentException(TransactionIdCannotBeEmptyExceptionMessage, nameof(transactionId));
		}

		TransactionId = transactionId;
	}
}
