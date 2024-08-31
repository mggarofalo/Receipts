using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Transaction;

public class CreateTransactionCommandHandler(ITransactionRepository transactionRepository) : IRequestHandler<CreateTransactionCommand, List<Domain.Core.Transaction>>
{
	private readonly ITransactionRepository _transactionRepository = transactionRepository;

	public async Task<List<Domain.Core.Transaction>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Transaction> createdEntities = await _transactionRepository.CreateAsync(request.Transactions, cancellationToken);
		await _transactionRepository.SaveChangesAsync(cancellationToken);
		return createdEntities;
	}
}