using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountsByReceiptIdQueryHandler(
	ITransactionService transactionService
) : IRequestHandler<GetTransactionAccountsByReceiptIdQuery, List<Domain.Aggregates.TransactionAccount>?>
{
	public async ValueTask<List<Domain.Aggregates.TransactionAccount>?> Handle(GetTransactionAccountsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await transactionService.GetTransactionAccountsByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
