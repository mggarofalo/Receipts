using Application.Interfaces.Services;
using Application.Models;
using MediatR;

namespace Application.Queries.Core.Card;

public class GetAllCardsQueryHandler(ICardService cardService) : IRequestHandler<GetAllCardsQuery, PagedResult<Domain.Core.Card>>
{
	public async Task<PagedResult<Domain.Core.Card>> Handle(GetAllCardsQuery request, CancellationToken cancellationToken)
	{
		return await cardService.GetAllAsync(request.Offset, request.Limit, request.Sort, request.IsActive, cancellationToken);
	}
}
