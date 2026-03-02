using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Adjustment;

public class GetAllAdjustmentsQueryHandler(IAdjustmentService adjustmentService) : IRequestHandler<GetAllAdjustmentsQuery, List<Domain.Core.Adjustment>>
{
	public async Task<List<Domain.Core.Adjustment>> Handle(GetAllAdjustmentsQuery request, CancellationToken cancellationToken)
	{
		return await adjustmentService.GetAllAsync(cancellationToken);
	}
}
