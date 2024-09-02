using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Core.Transaction;

public class GetTransactionByIdQueryHandler(ITransactionRepository transactionRepository) : IRequestHandler<GetTransactionByIdQuery, Domain.Core.Transaction?>
{
	public async Task<Domain.Core.Transaction?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
	{
		return await transactionRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}
