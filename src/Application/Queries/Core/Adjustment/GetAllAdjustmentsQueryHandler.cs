using Application.Interfaces.Services;
using Application.Models;
using Mediator;

namespace Application.Queries.Core.Adjustment;

public class GetAllAdjustmentsQueryHandler(IAdjustmentService adjustmentService) : IRequestHandler<GetAllAdjustmentsQuery, PagedResult<Domain.Core.Adjustment>>
{
	public async ValueTask<PagedResult<Domain.Core.Adjustment>> Handle(GetAllAdjustmentsQuery request, CancellationToken cancellationToken)
	{
		return await adjustmentService.GetAllAsync(request.Offset, request.Limit, request.Sort, cancellationToken);
	}
}
