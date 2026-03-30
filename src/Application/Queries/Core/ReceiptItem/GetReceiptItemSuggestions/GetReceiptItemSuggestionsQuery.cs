using Application.Interfaces;

namespace Application.Queries.Core.ReceiptItem.GetReceiptItemSuggestions;

public record GetReceiptItemSuggestionsQuery(string ItemCode, string? Location, int Limit = 10) : IQuery<IEnumerable<ReceiptItemSuggestion>>;
