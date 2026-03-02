using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Adjustment.Restore;

public class RestoreAdjustmentCommandHandler(IAdjustmentService adjustmentService) : IRequestHandler<RestoreAdjustmentCommand, bool>
{
	public async Task<bool> Handle(RestoreAdjustmentCommand request, CancellationToken cancellationToken)
	{
		return await adjustmentService.RestoreAsync(request.Id, cancellationToken);
	}
}
