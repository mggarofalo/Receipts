using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Transaction;

public class GetAllTransactionsQueryHandler(ITransactionService transactionRepository) : IRequestHandler<GetAllTransactionsQuery, List<Domain.Core.Transaction>>
{
	public async Task<List<Domain.Core.Transaction>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
	{
		return await transactionRepository.GetAllAsync(cancellationToken);
	}
}
