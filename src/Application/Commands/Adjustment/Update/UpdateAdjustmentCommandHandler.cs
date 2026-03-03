using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Adjustment.Update;

public class UpdateAdjustmentCommandHandler(IAdjustmentService adjustmentService) : IRequestHandler<UpdateAdjustmentCommand, bool>
{
	public async Task<bool> Handle(UpdateAdjustmentCommand request, CancellationToken cancellationToken)
	{
		Domain.Core.Adjustment existingAdjustment = await adjustmentService.GetByIdAsync(request.Adjustments[0].Id, cancellationToken)
			?? throw new InvalidOperationException("Adjustment not found");

		await adjustmentService.UpdateAsync([.. request.Adjustments], existingAdjustment.ReceiptId, cancellationToken);
		return true;
	}
}
