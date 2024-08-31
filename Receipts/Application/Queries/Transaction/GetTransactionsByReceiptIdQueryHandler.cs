using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Transaction;

public class GetTransactionsByReceiptIdQueryHandler(ITransactionRepository transactionRepository) : IRequestHandler<GetTransactionsByReceiptIdQuery, List<Domain.Core.Transaction>>
{
	private readonly ITransactionRepository _transactionRepository = transactionRepository;

	public async Task<List<Domain.Core.Transaction>> Handle(GetTransactionsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await _transactionRepository.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
