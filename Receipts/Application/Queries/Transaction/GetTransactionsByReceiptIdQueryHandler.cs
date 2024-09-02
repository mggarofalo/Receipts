using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Transaction;

public class GetTransactionsByReceiptIdQueryHandler(ITransactionRepository transactionRepository) : IRequestHandler<GetTransactionsByReceiptIdQuery, List<Domain.Core.Transaction>?>
{
	public async Task<List<Domain.Core.Transaction>?> Handle(GetTransactionsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await transactionRepository.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
