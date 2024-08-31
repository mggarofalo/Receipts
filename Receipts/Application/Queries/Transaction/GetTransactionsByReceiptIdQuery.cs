using Application.Interfaces;

namespace Application.Queries.Transaction;

public record GetTransactionsByReceiptIdQuery(Guid ReceiptId) : IQuery<List<Domain.Core.Transaction>>;
