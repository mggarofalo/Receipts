using Application.Interfaces;
using MediatR;

namespace Application.Queries.Receipt;

public record GetReceiptsByDateRangeQuery(DateOnly StartDate, DateOnly EndDate) : IQuery<List<Domain.Core.Receipt>>;

public class GetReceiptsByDateRangeQueryHandler(IReceiptRepository receiptRepository) : IRequestHandler<GetReceiptsByDateRangeQuery, List<Domain.Core.Receipt>>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;

	public async Task<List<Domain.Core.Receipt>> Handle(GetReceiptsByDateRangeQuery request, CancellationToken cancellationToken)
	{
		return await _receiptRepository.GetByDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);
	}
}
