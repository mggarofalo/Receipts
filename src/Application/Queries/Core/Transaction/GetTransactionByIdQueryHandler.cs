using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.Transaction;

public class GetTransactionByIdQueryHandler(ITransactionService transactionService) : IRequestHandler<GetTransactionByIdQuery, Domain.Core.Transaction?>
{
	public async ValueTask<Domain.Core.Transaction?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
	{
		return await transactionService.GetByIdAsync(request.Id, cancellationToken);
	}
}
