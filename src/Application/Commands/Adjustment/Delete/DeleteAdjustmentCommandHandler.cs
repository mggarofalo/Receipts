using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Adjustment.Delete;

public class DeleteAdjustmentCommandHandler(IAdjustmentService adjustmentService) : IRequestHandler<DeleteAdjustmentCommand, bool>
{
	public async Task<bool> Handle(DeleteAdjustmentCommand request, CancellationToken cancellationToken)
	{
		await adjustmentService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}
