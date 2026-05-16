using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.Core.Card;

public class GetCardByIdQueryHandler(ICardService cardService) : IRequestHandler<GetCardByIdQuery, Domain.Core.Card?>
{
	public async ValueTask<Domain.Core.Card?> Handle(GetCardByIdQuery request, CancellationToken cancellationToken)
	{
		return await cardService.GetByIdAsync(request.Id, cancellationToken);
	}
}
