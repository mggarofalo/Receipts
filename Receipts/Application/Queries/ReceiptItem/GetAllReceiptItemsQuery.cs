using Application.Interfaces;

namespace Application.Queries.ReceiptItem;

public record GetAllReceiptItemsQuery() : IQuery<List<Domain.Core.ReceiptItem>>;
