using Application.Interfaces;

namespace Application.Queries.Core.ReceiptItem;

public record GetDeletedReceiptItemsQuery() : IQuery<List<Domain.Core.ReceiptItem>>;
