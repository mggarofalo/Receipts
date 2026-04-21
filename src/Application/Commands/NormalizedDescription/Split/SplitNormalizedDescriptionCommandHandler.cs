using Application.Interfaces.Services;
using Domain.NormalizedDescriptions;
using MediatR;

namespace Application.Commands.NormalizedDescription.Split;

public class SplitNormalizedDescriptionCommandHandler(INormalizedDescriptionService service)
	: IRequestHandler<SplitNormalizedDescriptionCommand, Domain.NormalizedDescriptions.NormalizedDescription>
{
	public async Task<Domain.NormalizedDescriptions.NormalizedDescription> Handle(SplitNormalizedDescriptionCommand request, CancellationToken cancellationToken)
	{
		return await service.SplitAsync(request.ReceiptItemId, cancellationToken);
	}
}
