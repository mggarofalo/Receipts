using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.ReceiptItem;

public record GetAllReceiptItemsQuery(int Offset, int Limit, SortParams Sort, string? Q = null) : IQuery<PagedResult<Domain.Core.ReceiptItem>>;
