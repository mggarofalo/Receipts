using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Card.Delete;

public class DeleteCardCommandHandler(ICardService cardService) : IRequestHandler<DeleteCardCommand, bool>
{
	public async ValueTask<bool> Handle(DeleteCardCommand request, CancellationToken cancellationToken)
	{
		bool exists = await cardService.ExistsAsync(request.Id, cancellationToken);
		if (!exists)
		{
			return false;
		}

		await cardService.DeleteAsync(request.Id, cancellationToken);
		return true;
	}
}
