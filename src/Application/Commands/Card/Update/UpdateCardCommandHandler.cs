using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Card.Update;

public class UpdateCardCommandHandler(ICardService cardService) : IRequestHandler<UpdateCardCommand, bool>
{
	public async ValueTask<bool> Handle(UpdateCardCommand request, CancellationToken cancellationToken)
	{
		await cardService.UpdateAsync([.. request.Cards], cancellationToken);
		return true;
	}
}
