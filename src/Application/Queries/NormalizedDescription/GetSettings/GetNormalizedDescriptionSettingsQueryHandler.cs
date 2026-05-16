using Application.Interfaces.Services;
using Domain.NormalizedDescriptions;
using Mediator;

namespace Application.Queries.NormalizedDescription.GetSettings;

public class GetNormalizedDescriptionSettingsQueryHandler(INormalizedDescriptionService service)
	: IRequestHandler<GetNormalizedDescriptionSettingsQuery, NormalizedDescriptionSettings>
{
	public async ValueTask<NormalizedDescriptionSettings> Handle(GetNormalizedDescriptionSettingsQuery request, CancellationToken cancellationToken)
	{
		return await service.GetSettingsAsync(cancellationToken);
	}
}
