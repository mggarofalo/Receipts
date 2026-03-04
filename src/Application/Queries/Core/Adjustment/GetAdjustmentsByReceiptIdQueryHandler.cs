using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.Adjustment;

public class GetAdjustmentsByReceiptIdQueryHandler(IAdjustmentService adjustmentService) : IRequestHandler<GetAdjustmentsByReceiptIdQuery, PagedResult<Domain.Core.Adjustment>>
{
	public async Task<PagedResult<Domain.Core.Adjustment>> Handle(GetAdjustmentsByReceiptIdQuery request, CancellationToken cancellationToken)
	{
		return await adjustmentService.GetByReceiptIdAsync(request.ReceiptId, request.Offset, request.Limit, cancellationToken);
	}
}
