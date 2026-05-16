using Application.Interfaces.Services;
using Mediator;

namespace Application.Queries.NormalizedDescription.GetAll;

public class GetAllNormalizedDescriptionsQueryHandler(INormalizedDescriptionService service)
	: IRequestHandler<GetAllNormalizedDescriptionsQuery, List<Domain.NormalizedDescriptions.NormalizedDescription>>
{
	public async ValueTask<List<Domain.NormalizedDescriptions.NormalizedDescription>> Handle(GetAllNormalizedDescriptionsQuery request, CancellationToken cancellationToken)
	{
		return await service.GetAllAsync(request.StatusFilter, cancellationToken);
	}
}
