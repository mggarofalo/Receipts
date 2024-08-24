using Application.Interfaces;
using Domain;
using MediatR;

namespace Application.Queries.Receipt;

public record GetReceiptsByMoneyRangeQuery(Money MinAmount, Money MaxAmount) : IQuery<List<Domain.Core.Receipt>>;

public class GetReceiptsByMoneyRangeQueryHandler(IReceiptRepository receiptRepository) : IRequestHandler<GetReceiptsByMoneyRangeQuery, List<Domain.Core.Receipt>>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;

	public async Task<List<Domain.Core.Receipt>> Handle(GetReceiptsByMoneyRangeQuery request, CancellationToken cancellationToken)
	{
		return await _receiptRepository.GetByMoneyRangeAsync(request.MinAmount, request.MaxAmount, cancellationToken);
	}
}
