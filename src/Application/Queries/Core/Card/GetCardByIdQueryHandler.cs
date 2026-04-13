using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.Core.Card;

public class GetCardByIdQueryHandler(ICardService cardService) : IRequestHandler<GetCardByIdQuery, Domain.Core.Card?>
{
	public async Task<Domain.Core.Card?> Handle(GetCardByIdQuery request, CancellationToken cancellationToken)
	{
		return await cardService.GetByIdAsync(request.Id, cancellationToken);
	}
}
