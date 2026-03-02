using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Adjustment;

public class GetAdjustmentsByReceiptIdQueryHandler(IAdjustmentService adjustmentService) : IRequestHandler<GetAdjustmentsByReceiptIdQuery, List<Domain.Core.Adjustment>?>
{
	public async Task<List<Domain.Core.Adjustment>?> Handle(GetAdjustmentsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await adjustmentService.GetByReceiptIdAsync(request.ReceiptId, cancellationToken);
	}
}
