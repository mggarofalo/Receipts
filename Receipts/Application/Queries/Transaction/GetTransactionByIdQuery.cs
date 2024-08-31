using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Transaction;

public record GetTransactionByIdQuery(Guid Id) : IQuery<Domain.Core.Transaction?>;

public class GetTransactionByIdQueryHandler(ITransactionRepository transactionRepository) : IRequestHandler<GetTransactionByIdQuery, Domain.Core.Transaction?>
{
	private readonly ITransactionRepository _transactionRepository = transactionRepository;

	public async Task<Domain.Core.Transaction?> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
	{
		return await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);
	}
}
