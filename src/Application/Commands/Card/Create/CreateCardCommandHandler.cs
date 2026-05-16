using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Card.Create;

public class CreateCardCommandHandler(ICardService cardService) : IRequestHandler<CreateCardCommand, List<Domain.Core.Card>>
{
	public async ValueTask<List<Domain.Core.Card>> Handle(CreateCardCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Card> createdEntities = await cardService.CreateAsync([.. request.Cards], cancellationToken);
		return createdEntities;
	}
}
