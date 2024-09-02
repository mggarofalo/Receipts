using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Transaction;

public class DeleteTransactionCommandHandler(ITransactionRepository transactionRepository) : IRequestHandler<DeleteTransactionCommand, bool>
{
	private readonly ITransactionRepository _transactionRepository = transactionRepository;

	public async Task<bool> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
	{
		await _transactionRepository.DeleteAsync([.. request.Ids], cancellationToken);
		await _transactionRepository.SaveChangesAsync(cancellationToken);
		return true;
	}
}