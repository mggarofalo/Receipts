using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Trash.Purge;

public class PurgeTrashCommandHandler(ITrashService trashService) : IRequestHandler<PurgeTrashCommand, bool>
{
	public async Task<bool> Handle(PurgeTrashCommand request, CancellationToken cancellationToken)
	{
		await trashService.PurgeAllDeletedAsync(cancellationToken);
		return true;
	}
}
