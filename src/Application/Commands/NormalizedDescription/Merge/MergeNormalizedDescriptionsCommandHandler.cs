using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.NormalizedDescription.Merge;

public class MergeNormalizedDescriptionsCommandHandler(INormalizedDescriptionService service)
	: IRequestHandler<MergeNormalizedDescriptionsCommand, int>
{
	public async Task<int> Handle(MergeNormalizedDescriptionsCommand request, CancellationToken cancellationToken)
	{
		return await service.MergeAsync(request.KeepId, request.DiscardId, cancellationToken);
	}
}
