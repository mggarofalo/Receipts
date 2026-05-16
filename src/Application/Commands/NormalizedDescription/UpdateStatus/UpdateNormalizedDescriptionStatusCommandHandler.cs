using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.NormalizedDescription.UpdateStatus;

public class UpdateNormalizedDescriptionStatusCommandHandler(INormalizedDescriptionService service)
	: IRequestHandler<UpdateNormalizedDescriptionStatusCommand, bool>
{
	public async ValueTask<bool> Handle(UpdateNormalizedDescriptionStatusCommand request, CancellationToken cancellationToken)
	{
		return await service.UpdateStatusAsync(request.Id, request.Status, cancellationToken);
	}
}
