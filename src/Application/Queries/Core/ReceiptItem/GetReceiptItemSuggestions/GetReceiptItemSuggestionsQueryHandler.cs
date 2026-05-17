using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.ReceiptItem.GetReceiptItemSuggestions;

public class GetReceiptItemSuggestionsQueryHandler(IReceiptItemService receiptItemService) : IRequestHandler<GetReceiptItemSuggestionsQuery, IEnumerable<ReceiptItemSuggestion>>
{
	public async ValueTask<IEnumerable<ReceiptItemSuggestion>> Handle(GetReceiptItemSuggestionsQuery request, CancellationToken cancellationToken)
	{
		return await receiptItemService.GetSuggestionsAsync(request.ItemCode, request.Location, request.Limit, cancellationToken);
	}
}
