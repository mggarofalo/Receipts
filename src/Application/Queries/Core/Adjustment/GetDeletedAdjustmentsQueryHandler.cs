using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Adjustment;

public class GetDeletedAdjustmentsQueryHandler(IAdjustmentService adjustmentService) : IRequestHandler<GetDeletedAdjustmentsQuery, List<Domain.Core.Adjustment>>
{
	public async Task<List<Domain.Core.Adjustment>> Handle(GetDeletedAdjustmentsQuery request, CancellationToken cancellationToken)
	{
		return await adjustmentService.GetDeletedAsync(cancellationToken);
	}
}
