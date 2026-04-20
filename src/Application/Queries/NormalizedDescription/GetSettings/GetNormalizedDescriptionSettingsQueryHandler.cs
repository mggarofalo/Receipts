using Application.Interfaces.Services;
using Domain.NormalizedDescriptions;
using MediatR;

namespace Application.Queries.NormalizedDescription.GetSettings;

public class GetNormalizedDescriptionSettingsQueryHandler(INormalizedDescriptionService service)
	: IRequestHandler<GetNormalizedDescriptionSettingsQuery, NormalizedDescriptionSettings>
{
	public async Task<NormalizedDescriptionSettings> Handle(GetNormalizedDescriptionSettingsQuery request, CancellationToken cancellationToken)
	{
		return await service.GetSettingsAsync(cancellationToken);
	}
}
