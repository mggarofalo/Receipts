using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.Transaction;

public class GetTransactionsByReceiptIdQueryHandler(ITransactionService transactionService) : IRequestHandler<GetTransactionsByReceiptIdQuery, PagedResult<Domain.Core.Transaction>>
{
	public async ValueTask<PagedResult<Domain.Core.Transaction>> Handle(GetTransactionsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await transactionService.GetByReceiptIdAsync(request.ReceiptId, request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
