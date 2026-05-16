using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.Transaction;

public class GetDeletedTransactionsQueryHandler(ITransactionService transactionService) : IRequestHandler<GetDeletedTransactionsQuery, PagedResult<Domain.Core.Transaction>>
{
	public async ValueTask<PagedResult<Domain.Core.Transaction>> Handle(GetDeletedTransactionsQuery request, CancellationToken cancellationToken)
	{
		return await transactionService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
