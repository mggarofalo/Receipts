using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Reports;

public class RefreshItemSimilarityCommandHandler(IDescriptionChangeSignal signal)
	: IRequestHandler<RefreshItemSimilarityCommand, Unit>
{
	public ValueTask<Unit> Handle(RefreshItemSimilarityCommand request, CancellationToken cancellationToken)
	{
		signal.NotifyDirty();
		return ValueTask.FromResult(Unit.Value);
	}
}
