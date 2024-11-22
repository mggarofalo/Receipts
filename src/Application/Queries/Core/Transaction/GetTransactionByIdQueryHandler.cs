using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Transaction;

public class GetTransactionByIdQueryHandler(ITransactionService transactionService) : IRequestHandler<GetTransactionByIdQuery, Domain.Core.Transaction?>
{
	public async Task<Domain.Core.Transaction?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
	{
		return await transactionService.GetByIdAsync(request.Id, cancellationToken);
	}
}
