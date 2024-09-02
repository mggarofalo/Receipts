using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Transaction;

public class GetAllTransactionsQueryHandler(ITransactionRepository transactionRepository) : IRequestHandler<GetAllTransactionsQuery, List<Domain.Core.Transaction>>
{
	public async Task<List<Domain.Core.Transaction>> Handle(GetAllTransactionsQuery request, CancellationToken cancellationToken)
	{
		return await transactionRepository.GetAllAsync(cancellationToken);
	}
}
