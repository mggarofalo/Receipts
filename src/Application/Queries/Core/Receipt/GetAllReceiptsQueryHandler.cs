using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Receipt;

public class GetAllReceiptsQueryHandler(IReceiptService receiptService) : IRequestHandler<GetAllReceiptsQuery, List<Domain.Core.Receipt>>
{
	public async Task<List<Domain.Core.Receipt>> Handle(GetAllReceiptsQuery request, CancellationToken cancellationToken)
	{
		return await receiptService.GetAllAsync(cancellationToken);
	}
}
