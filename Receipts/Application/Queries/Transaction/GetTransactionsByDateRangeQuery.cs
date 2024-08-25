using Application.Interfaces;
using MediatR;

namespace Application.Queries.Transaction;

public record GetTransactionsByDateRangeQuery(DateOnly StartDate, DateOnly EndDate) : IQuery<List<Domain.Core.Transaction>>;

public class GetTransactionsByDateRangeQueryHandler(ITransactionRepository transactionRepository) : IRequestHandler<GetTransactionsByDateRangeQuery, List<Domain.Core.Transaction>>
{
	private readonly ITransactionRepository _transactionRepository = transactionRepository;

	public async Task<List<Domain.Core.Transaction>> Handle(GetTransactionsByDateRangeQuery request, CancellationToken cancellationToken)
	{
		return await _transactionRepository.GetByDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);
	}
}
