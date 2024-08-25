using Application.Interfaces;
using Domain;
using MediatR;

namespace Application.Queries.Transaction;

public record GetTransactionsByMoneyRangeQuery(Money MinAmount, Money MaxAmount) : IQuery<List<Domain.Core.Transaction>>;

public class GetTransactionsByMoneyRangeQueryHandler(ITransactionRepository transactionRepository) : IRequestHandler<GetTransactionsByMoneyRangeQuery, List<Domain.Core.Transaction>>
{
	private readonly ITransactionRepository _transactionRepository = transactionRepository;

	public async Task<List<Domain.Core.Transaction>> Handle(GetTransactionsByMoneyRangeQuery request, CancellationToken cancellationToken)
	{
		return await _transactionRepository.GetByMoneyRangeAsync(request.MinAmount, request.MaxAmount, cancellationToken);
	}
}
