using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Adjustment.Create;

public class CreateAdjustmentCommandHandler(IAdjustmentService adjustmentService) : IRequestHandler<CreateAdjustmentCommand, List<Domain.Core.Adjustment>>
{
	public async Task<List<Domain.Core.Adjustment>> Handle(CreateAdjustmentCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Adjustment> createdEntities = await adjustmentService.CreateAsync([.. request.Adjustments], request.ReceiptId, cancellationToken);
		return createdEntities;
	}
}
