using Application.Interfaces;
using Application.Models;

namespace Application.Queries.Core.ReceiptItem;

public record GetDeletedReceiptItemsQuery(int Offset, int Limit) : IQuery<PagedResult<Domain.Core.ReceiptItem>>;
