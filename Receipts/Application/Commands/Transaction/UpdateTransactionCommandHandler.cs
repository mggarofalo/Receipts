using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Commands.Transaction;

public class UpdateTransactionCommandHandler(ITransactionRepository transactionRepository) : IRequestHandler<UpdateTransactionCommand, bool>
{
	public async Task<bool> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
	{
		await transactionRepository.UpdateAsync([.. request.Transactions], cancellationToken);
		await transactionRepository.SaveChangesAsync(cancellationToken);
		return true;
	}
}