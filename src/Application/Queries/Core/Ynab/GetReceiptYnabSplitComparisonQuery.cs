using Application.Models.Ynab;
using MediatR;

namespace Application.Queries.Core.Ynab;

public record GetReceiptYnabSplitComparisonQuery(Guid ReceiptId) : IRequest<ReceiptYnabSplitComparisonResult>;
