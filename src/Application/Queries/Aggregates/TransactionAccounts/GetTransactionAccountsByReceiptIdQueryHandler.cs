using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Aggregates.TransactionAccounts;

public class GetTransactionAccountsByReceiptIdQueryHandler(
	ITransactionService transactionService,
	IAccountService accountService
) : IRequestHandler<GetTransactionAccountsByReceiptIdQuery, List<Domain.Aggregates.TransactionAccount>?>
{
	public async Task<List<Domain.Aggregates.TransactionAccount>?> Handle(GetTransactionAccountsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Transaction>? transactions = await transactionService.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);

		if (transactions == null)
		{
			return null;
		}

		List<Domain.Aggregates.TransactionAccount> transactionAccounts = [];

		foreach (Domain.Core.Transaction transaction in transactions)
		{
			Domain.Core.Account? account = await accountService.GetByTransactionIdAsync(transaction.Id!.Value, cancellationToken);

			if (account == null)
			{
				continue;
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
