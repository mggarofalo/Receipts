using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.Adjustment;

public class GetAdjustmentByIdQueryHandler(IAdjustmentService adjustmentService) : IRequestHandler<GetAdjustmentByIdQuery, Domain.Core.Adjustment?>
{
	public async ValueTask<Domain.Core.Adjustment?> Handle(GetAdjustmentByIdQuery request, CancellationToken cancellationToken)
	{
		return await adjustmentService.GetByIdAsync(request.Id, cancellationToken);
	}
}
