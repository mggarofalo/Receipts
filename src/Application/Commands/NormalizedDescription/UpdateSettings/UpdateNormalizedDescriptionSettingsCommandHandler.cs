using Application.Interfaces.Services;
using Domain.NormalizedDescriptions;
using Mediator;

namespace Application.Commands.NormalizedDescription.UpdateSettings;

public class UpdateNormalizedDescriptionSettingsCommandHandler(INormalizedDescriptionService service)
	: IRequestHandler<UpdateNormalizedDescriptionSettingsCommand, NormalizedDescriptionSettings>
{
	public async ValueTask<NormalizedDescriptionSettings> Handle(UpdateNormalizedDescriptionSettingsCommand request, CancellationToken cancellationToken)
	{
		return await service.UpdateSettingsAsync(
			request.AutoAcceptThreshold,
			request.PendingReviewThreshold,
			cancellationToken);
	}
}
