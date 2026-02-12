using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Transaction;

public class GetAllTransactionsQueryHandler(ITransactionService transactionService) : IRequestHandler<GetAllTransactionsQuery, List<Domain.Core.Transaction>>
{
	public async Task<List<Domain.Core.Transaction>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
	{
		return await transactionService.GetAllAsync(cancellationToken);
	}
}
