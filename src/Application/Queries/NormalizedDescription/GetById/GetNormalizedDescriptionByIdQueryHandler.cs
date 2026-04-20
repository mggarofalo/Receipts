using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.NormalizedDescription.GetById;

public class GetNormalizedDescriptionByIdQueryHandler(INormalizedDescriptionService service)
	: IRequestHandler<GetNormalizedDescriptionByIdQuery, Domain.NormalizedDescriptions.NormalizedDescription?>
{
	public async Task<Domain.NormalizedDescriptions.NormalizedDescription?> Handle(GetNormalizedDescriptionByIdQuery request, CancellationToken cancellationToken)
	{
		return await service.GetByIdAsync(request.Id, cancellationToken);
	}
}
