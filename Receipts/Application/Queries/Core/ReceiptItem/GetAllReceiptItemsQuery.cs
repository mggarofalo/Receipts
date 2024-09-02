using Application.Interfaces;

namespace Application.Queries.Core.ReceiptItem;

public record GetAllReceiptItemsQuery() : IQuery<List<Domain.Core.ReceiptItem>>;
