using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Transaction.Restore;

public class RestoreTransactionCommandHandler(ITransactionService transactionService) : IRequestHandler<RestoreTransactionCommand, bool>
{
	public async Task<bool> Handle(RestoreTransactionCommand request, CancellationToken cancellationToken)
	{
		return await transactionService.RestoreAsync(request.Id, cancellationToken);
	}
}
