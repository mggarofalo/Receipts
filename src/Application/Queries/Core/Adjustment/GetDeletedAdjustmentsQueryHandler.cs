using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.Adjustment;

public class GetDeletedAdjustmentsQueryHandler(IAdjustmentService adjustmentService) : IRequestHandler<GetDeletedAdjustmentsQuery, PagedResult<Domain.Core.Adjustment>>
{
	public async ValueTask<PagedResult<Domain.Core.Adjustment>> Handle(GetDeletedAdjustmentsQuery request, CancellationToken cancellationToken)
	{
		return await adjustmentService.GetDeletedAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
