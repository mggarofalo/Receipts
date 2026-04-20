using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.NormalizedDescription.UpdateStatus;

public class UpdateNormalizedDescriptionStatusCommandHandler(INormalizedDescriptionService service)
	: IRequestHandler<UpdateNormalizedDescriptionStatusCommand, bool>
{
	public async Task<bool> Handle(UpdateNormalizedDescriptionStatusCommand request, CancellationToken cancellationToken)
	{
		return await service.UpdateStatusAsync(request.Id, request.Status, cancellationToken);
	}
}
