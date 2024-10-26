using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Transaction;

public class UpdateTransactionCommandHandler(ITransactionService transactionService) : IRequestHandler<UpdateTransactionCommand, bool>
{
	public async Task<bool> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
	{
		await transactionService.UpdateAsync([.. request.Transactions], request.AccountId, request.ReceiptId, cancellationToken);
		return true;
	}
}