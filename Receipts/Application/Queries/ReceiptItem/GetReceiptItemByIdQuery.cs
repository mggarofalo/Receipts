using Application.Interfaces;

namespace Application.Queries.ReceiptItem;

public record GetReceiptItemByIdQuery(Guid Id) : IQuery<Domain.Core.ReceiptItem?>;
