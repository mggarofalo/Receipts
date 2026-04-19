using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Card;

public class GetCardsByAccountIdQueryHandler(ICardService cardService) : IRequestHandler<GetCardsByAccountIdQuery, List<Domain.Core.Card>>
{
	public async Task<List<Domain.Core.Card>> Handle(GetCardsByAccountIdQuery request, CancellationToken cancellationToken)
	{
		return await cardService.GetByAccountIdAsync(request.AccountId, cancellationToken);
	}
}
