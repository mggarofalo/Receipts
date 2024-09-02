using Application.Queries.Aggregates.ReceiptsWithItems;
using Application.Queries.Aggregates.TransactionAccounts;
using MediatR;

namespace Application.Queries.Aggregates.Trips;

public class GetTripByReceiptIdQueryHandler(IMediator mediator) : IRequestHandler<GetTripByReceiptIdQuery, Domain.Aggregates.Trip?>
{
	public async Task<Domain.Aggregates.Trip?> Handle(GetTripByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		GetReceiptWithItemsByReceiptIdQuery getReceiptWithItemsByReceiptIdQuery = new(request.ReceiptId);
		Domain.Aggregates.ReceiptWithItems? receiptWithItems = await mediator.Send(getReceiptWithItemsByReceiptIdQuery, cancellationToken);

		if (receiptWithItems == null)
		{
			return null;
		}

		GetTransactionAccountsByReceiptIdQuery getTransactionAccountsByReceiptIdQuery = new(request.ReceiptId);
		List<Domain.Aggregates.TransactionAccount>? transactionAccounts = await mediator.Send(getTransactionAccountsByReceiptIdQuery, cancellationToken);

		if (transactionAccounts == null)
		{
			return null;
		}

		return new Domain.Aggregates.Trip()
		{
			Receipt = receiptWithItems,
			Transactions = transactionAccounts
		};
	}
}