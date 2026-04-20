using Application.Interfaces.Services;
using Domain.NormalizedDescriptions;
using MediatR;

namespace Application.Commands.NormalizedDescription.UpdateSettings;

public class UpdateNormalizedDescriptionSettingsCommandHandler(INormalizedDescriptionService service)
	: IRequestHandler<UpdateNormalizedDescriptionSettingsCommand, NormalizedDescriptionSettings>
{
	public async Task<NormalizedDescriptionSettings> Handle(UpdateNormalizedDescriptionSettingsCommand request, CancellationToken cancellationToken)
	{
		return await service.UpdateSettingsAsync(
			request.AutoAcceptThreshold,
			request.PendingReviewThreshold,
			cancellationToken);
	}
}
