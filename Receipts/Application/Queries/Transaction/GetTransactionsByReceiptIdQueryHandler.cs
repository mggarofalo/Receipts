using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Transaction;

public class GetTransactionsByReceiptIdQueryHandler(ITransactionRepository transactionRepository) : IRequestHandler<GetTransactionsByReceiptIdQuery, List<Domain.Core.Transaction>>
{
	private readonly ITransactionRepository _transactionRepository = transactionRepository;

	public async Task<List<Domain.Core.Transaction>> Handle(GetTransactionsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		// TODO: Update repo to return null if receiptId doesn't exist and empty if receiptId exists but has no transactions
		return await _transactionRepository.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
