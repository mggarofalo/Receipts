using Application.Interfaces.Services;
using MediatR;

namespace Application.Queries.NormalizedDescription.GetAll;

public class GetAllNormalizedDescriptionsQueryHandler(INormalizedDescriptionService service)
	: IRequestHandler<GetAllNormalizedDescriptionsQuery, List<Domain.NormalizedDescriptions.NormalizedDescription>>
{
	public async Task<List<Domain.NormalizedDescriptions.NormalizedDescription>> Handle(GetAllNormalizedDescriptionsQuery request, CancellationToken cancellationToken)
	{
		return await service.GetAllAsync(request.StatusFilter, cancellationToken);
	}
}
