using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Trash.Purge;

public class PurgeTrashCommandHandler(ITrashService trashService) : IRequestHandler<PurgeTrashCommand, bool>
{
	public async ValueTask<bool> Handle(PurgeTrashCommand request, CancellationToken cancellationToken)
	{
		await trashService.PurgeAllDeletedAsync(cancellationToken);
		return true;
	}
}
