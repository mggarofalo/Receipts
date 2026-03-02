using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Adjustment.Update;

public class UpdateAdjustmentCommandHandler(IAdjustmentService adjustmentService) : IRequestHandler<UpdateAdjustmentCommand, bool>
{
	public async Task<bool> Handle(UpdateAdjustmentCommand request, CancellationToken cancellationToken)
	{
		await adjustmentService.UpdateAsync([.. request.Adjustments], request.ReceiptId, cancellationToken);
		return true;
	}
}
