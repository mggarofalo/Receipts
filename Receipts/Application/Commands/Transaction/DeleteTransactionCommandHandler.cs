using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Transaction;

public class DeleteTransactionCommandHandler(ITransactionRepository transactionRepository) : IRequestHandler<DeleteTransactionCommand, bool>
{
	public async Task<bool> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
	{
		await transactionRepository.DeleteAsync([.. request.Ids], cancellationToken);
		await transactionRepository.SaveChangesAsync(cancellationToken);
		return true;
	}
}