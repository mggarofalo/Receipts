using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Card.Create;

public class CreateCardCommandHandler(ICardService cardService) : IRequestHandler<CreateCardCommand, List<Domain.Core.Card>>
{
	public async Task<List<Domain.Core.Card>> Handle(CreateCardCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Card> createdEntities = await cardService.CreateAsync([.. request.Cards], cancellationToken);
		return createdEntities;
	}
}
