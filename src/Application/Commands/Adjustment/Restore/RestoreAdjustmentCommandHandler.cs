using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Adjustment.Restore;

public class RestoreAdjustmentCommandHandler(IAdjustmentService adjustmentService) : IRequestHandler<RestoreAdjustmentCommand, bool>
{
	public async ValueTask<bool> Handle(RestoreAdjustmentCommand request, CancellationToken cancellationToken)
	{
		return await adjustmentService.RestoreAsync(request.Id, cancellationToken);
	}
}
