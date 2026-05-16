using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Adjustment.Delete;

public class DeleteAdjustmentCommandHandler(IAdjustmentService adjustmentService) : IRequestHandler<DeleteAdjustmentCommand, bool>
{
	public async ValueTask<bool> Handle(DeleteAdjustmentCommand request, CancellationToken cancellationToken)
	{
		await adjustmentService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}
