using Application.Interfaces;

namespace Application.Queries.ReceiptItem;

public record GetReceiptItemsByReceiptIdQuery(Guid ReceiptId) : IQuery<List<Domain.Core.ReceiptItem>>;
