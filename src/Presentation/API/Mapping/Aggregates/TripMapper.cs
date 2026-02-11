using Domain.Aggregates;
using Riok.Mapperly.Abstractions;
using Shared.ViewModels.Aggregates;

namespace API.Mapping.Aggregates;

[Mapper]
public partial class TripMapper
{
	private readonly ReceiptWithItemsMapper _receiptWithItemsMapper = new();
	private readonly TransactionAccountMapper _transactionAccountMapper = new();

	public TripVM ToViewModel(Trip source)
	{
		return new TripVM
		{
			Receipt = _receiptWithItemsMapper.ToViewModel(source.Receipt),
			Transactions = source.Transactions.Select(_transactionAccountMapper.ToViewModel).ToList()
		};
	}

	public Trip ToDomain(TripVM source)
	{
		return new Trip
		{
			Receipt = _receiptWithItemsMapper.ToDomain(source.Receipt!),
			Transactions = source.Transactions!.Select(_transactionAccountMapper.ToDomain).ToList()
		};
	}
}
