using Application.Models.Ynab;
using Mediator;

namespace Application.Queries.Core.Ynab;

public record GetReceiptYnabSplitComparisonQuery(Guid ReceiptId) : IRequest<ReceiptYnabSplitComparisonResult>;
