using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Transaction.Delete;

public class DeleteTransactionCommandHandler(ITransactionService transactionService) : IRequestHandler<DeleteTransactionCommand, bool>
{
	public async ValueTask<bool> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
	{
		await transactionService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}