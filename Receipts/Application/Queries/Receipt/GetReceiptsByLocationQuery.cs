using Application.Interfaces;
using MediatR;

namespace Application.Queries.Receipt;

public record GetReceiptsByLocationQuery(string Location) : IQuery<List<Domain.Core.Receipt>>;

public class GetReceiptsByLocationQueryHandler(IReceiptRepository receiptRepository) : IRequestHandler<GetReceiptsByLocationQuery, List<Domain.Core.Receipt>>
{
	private readonly IReceiptRepository _receiptRepository = receiptRepository;

	public async Task<List<Domain.Core.Receipt>> Handle(GetReceiptsByLocationQuery request, CancellationToken cancellationToken)
	{
		return await _receiptRepository.GetByLocationAsync(request.Location, cancellationToken);
	}
}
