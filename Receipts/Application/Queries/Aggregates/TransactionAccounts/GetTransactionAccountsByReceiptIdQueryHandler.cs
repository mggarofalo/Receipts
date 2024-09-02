using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountsByReceiptIdQueryHandler(
	ITransactionRepository transactionRepository,
	IAccountRepository accountRepository
) : IRequestHandler<GetTransactionAccountsByReceiptIdQuery, List<Domain.Aggregates.TransactionAccount>?>
{
	public async Task<List<Domain.Aggregates.TransactionAccount>?> Handle(GetTransactionAccountsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Transaction>? transactions = await transactionRepository.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);

		if (transactions == null)
		{
			return null;
		}

		List<Domain.Aggregates.TransactionAccount> transactionAccounts = [];

		foreach (Domain.Core.Transaction transaction in transactions)
		{
			Domain.Core.Account? account = await accountRepository.GetByIdAsync(transaction.AccountId, cancellationToken);

			if (account == null)
			{
				return null;
			}

			transactionAccounts.Add(new Domain.Aggregates.TransactionAccount()
			{
				Transaction = transaction,
				Account = account
			});
		}

		return transactionAccounts;
	}
}
