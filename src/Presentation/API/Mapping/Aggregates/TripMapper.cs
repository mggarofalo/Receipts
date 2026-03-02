using API.Generated.Dtos;
using Domain.Aggregates;
using Riok.Mapperly.Abstractions;

namespace API.Mapping.Aggregates;

[Mapper]
public partial class TripMapper
{
	private readonly ReceiptWithItemsMapper _receiptWithItemsMapper = new();
	private readonly TransactionAccountMapper _transactionAccountMapper = new();

	public TripResponse ToResponse(Trip source)
	{
		return new TripResponse
		{
			Receipt = _receiptWithItemsMapper.ToResponse(source.Receipt),
			Transactions = [.. source.Transactions.Select(_transactionAccountMapper.ToResponse)],
			Warnings = [.. source.GetWarnings().Select(MapWarning)]
		};
	}

	private static ValidationWarningResponse MapWarning(Domain.ValidationWarning w) => new()
	{
		Property = w.Property,
		Message = w.Message,
		Severity = (int)w.Severity
	};
}
