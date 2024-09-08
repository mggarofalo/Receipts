using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Transaction;

public class CreateTransactionCommandHandler(ITransactionRepository transactionRepository) : IRequestHandler<CreateTransactionCommand, List<Domain.Core.Transaction>>
{
	public async Task<List<Domain.Core.Transaction>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Transaction> createdEntities = await transactionRepository.CreateAsync([.. request.Transactions], request.AccountId, request.ReceiptId, cancellationToken);
		await transactionRepository.SaveChangesAsync(cancellationToken);
		return createdEntities;
	}
}