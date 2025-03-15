using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Transaction.Create;

public class CreateTransactionCommandHandler(ITransactionService transactionService) : IRequestHandler<CreateTransactionCommand, List<Domain.Core.Transaction>>
{
	public async Task<List<Domain.Core.Transaction>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Transaction> createdEntities = await transactionService.CreateAsync([.. request.Transactions], request.AccountId, request.ReceiptId, cancellationToken);
		return createdEntities;
	}
}