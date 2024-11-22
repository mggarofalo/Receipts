using Application.Queries.Aggregates.ReceiptsWithItems;
using Application.Queries.Aggregates.TransactionAccounts;
using MediatR;

namespace Application.Queries.Aggregates.Trips;

public class GetTripByReceiptIdQueryHandler(IMediator mediator) : IRequestHandler<GetTripByReceiptIdQuery, Domain.Aggregates.Trip?>
{
	public async Task<Domain.Aggregates.Trip?> Handle(GetTripByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		GetReceiptWithItemsByReceiptIdQuery getReceiptWithItemsByReceiptIdQuery = new(request.ReceiptId);
		Task<Domain.Aggregates.ReceiptWithItems?> receiptWithItemsTask = mediator.Send(getReceiptWithItemsByReceiptIdQuery, cancellationToken);

		GetTransactionAccountsByReceiptIdQuery getTransactionAccountsByReceiptIdQuery = new(request.ReceiptId);
		Task<List<Domain.Aggregates.TransactionAccount>?> transactionAccountsTask = mediator.Send(getTransactionAccountsByReceiptIdQuery, cancellationToken);

		await Task.WhenAll(receiptWithItemsTask, transactionAccountsTask);

		if (receiptWithItemsTask.Result == null)
		{
			return null;
		}

		if (transactionAccountsTask.Result == null)
		{
			return null;
		}

		return new Domain.Aggregates.Trip()
		{
			Receipt = receiptWithItemsTask.Result,
			Transactions = transactionAccountsTask.Result
		};
	}
}