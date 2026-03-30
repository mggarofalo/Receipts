using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.ReceiptItem.GetReceiptItemSuggestions;

public class GetReceiptItemSuggestionsQueryHandler(IReceiptItemService receiptItemService) : IRequestHandler<GetReceiptItemSuggestionsQuery, IEnumerable<ReceiptItemSuggestion>>
{
	public async Task<IEnumerable<ReceiptItemSuggestion>> Handle(GetReceiptItemSuggestionsQuery request, CancellationToken cancellationToken)
	{
		return await receiptItemService.GetSuggestionsAsync(request.ItemCode, request.Location, request.Limit, cancellationToken);
	}
}
