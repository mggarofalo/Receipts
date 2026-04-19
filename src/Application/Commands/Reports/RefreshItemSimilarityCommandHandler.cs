using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Reports;

public class RefreshItemSimilarityCommandHandler(IDescriptionChangeSignal signal)
	: IRequestHandler<RefreshItemSimilarityCommand, Unit>
{
	public Task<Unit> Handle(RefreshItemSimilarityCommand request, CancellationToken cancellationToken)
	{
		signal.NotifyDirty();
		return Task.FromResult(Unit.Value);
	}
}
