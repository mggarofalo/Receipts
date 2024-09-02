using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountByTransactionIdQueryHandler(
	ITransactionRepository transactionRepository,
	IAccountRepository accountRepository
) : IRequestHandler<GetTransactionAccountByTransactionIdQuery, Domain.Aggregates.TransactionAccount?>
{
	public async Task<Domain.Aggregates.TransactionAccount?> Handle(GetTransactionAccountByTransactionIdQuery request, CancellationToken cancellationToken)
	{
		Domain.Core.Transaction? transaction = await transactionRepository.GetByIdAsync(request.TransactionId, cancellationToken);

		if (transaction == null)
		{
			return null;
		}

		Domain.Core.Account? account = await accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);

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
