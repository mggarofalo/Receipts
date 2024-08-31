using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Transaction;

public class UpdateTransactionCommandHandler(ITransactionRepository transactionRepository) : IRequestHandler<UpdateTransactionCommand, bool>
{
	private readonly ITransactionRepository _transactionRepository = transactionRepository;

	public async Task<bool> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
	{
		bool success = await _transactionRepository.UpdateAsync(request.Transactions, cancellationToken);

		if (success)
		{
			await _transactionRepository.SaveChangesAsync(cancellationToken);
		}

		return success;
	}
}