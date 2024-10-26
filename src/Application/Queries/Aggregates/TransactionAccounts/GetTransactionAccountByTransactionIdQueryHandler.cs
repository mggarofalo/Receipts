using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountByTransactionIdQueryHandler(
	ITransactionService transactionRepository,
	IAccountService accountRepository
) : IRequestHandler<GetTransactionAccountByTransactionIdQuery, Domain.Aggregates.TransactionAccount?>
{
	public async Task<Domain.Aggregates.TransactionAccount?> Handle(GetTransactionAccountByTransactionIdQuery request, CancellationToken cancellationToken)
	{
		Domain.Core.Transaction? transaction = await transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);
		Domain.Core.Account? account = await accountRepository.GetByTransactionIdAsync(request.TransactionId, cancellationToken);

		if (transaction == null)
		{
			return null;
		}

		if (account == null)
		{
			return null;
		}

		return new Domain.Aggregates.TransactionAccount()
		{
			Transaction = transaction,
			Account = account
		};
	}
}
