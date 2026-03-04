using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.Transaction;

public class GetAllTransactionsQueryHandler(ITransactionService transactionService) : IRequestHandler<GetAllTransactionsQuery, PagedResult<Domain.Core.Transaction>>
{
	public async Task<PagedResult<Domain.Core.Transaction>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
	{
		return await transactionService.GetAllAsync(request.Offset, request.Limit, cancellationToken);
	}
}
