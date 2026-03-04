using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountsByReceiptIdQueryHandler(
	ITransactionService transactionService
) : IRequestHandler<GetTransactionAccountsByReceiptIdQuery, List<Domain.Aggregates.TransactionAccount>?>
{
	public async Task<List<Domain.Aggregates.TransactionAccount>?> Handle(GetTransactionAccountsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await transactionService.GetTransactionAccountsByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
