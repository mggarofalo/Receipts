using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Transaction.Restore;

public class RestoreTransactionCommandHandler(ITransactionService transactionService) : IRequestHandler<RestoreTransactionCommand, bool>
{
	public async ValueTask<bool> Handle(RestoreTransactionCommand request, CancellationToken cancellationToken)
	{
		return await transactionService.RestoreAsync(request.Id, cancellationToken);
	}
}
