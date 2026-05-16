using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.Receipt;

public class GetDeletedReceiptsQueryHandler(IReceiptService receiptService) : IRequestHandler<GetDeletedReceiptsQuery, PagedResult<Domain.Core.Receipt>>
{
	public async ValueTask<PagedResult<Domain.Core.Receipt>> Handle(GetDeletedReceiptsQuery request, CancellationToken cancellationToken)
	{
		return await receiptService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
