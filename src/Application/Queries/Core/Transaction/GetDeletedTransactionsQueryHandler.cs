using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Transaction;

public class GetDeletedTransactionsQueryHandler(ITransactionService transactionService) : IRequestHandler<GetDeletedTransactionsQuery, List<Domain.Core.Transaction>>
{
	public async Task<List<Domain.Core.Transaction>> Handle(GetDeletedTransactionsQuery request, CancellationToken cancellationToken)
	{
		return await transactionService.GetDeletedAsync(cancellationToken);
	}
}
