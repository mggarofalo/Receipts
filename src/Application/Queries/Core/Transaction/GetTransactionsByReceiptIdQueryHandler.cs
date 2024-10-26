using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Transaction;

public class GetTransactionsByReceiptIdQueryHandler(ITransactionService transactionRepository) : IRequestHandler<GetTransactionsByReceiptIdQuery, List<Domain.Core.Transaction>?>
{
	public async Task<List<Domain.Core.Transaction>?> Handle(GetTransactionsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await transactionRepository.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
