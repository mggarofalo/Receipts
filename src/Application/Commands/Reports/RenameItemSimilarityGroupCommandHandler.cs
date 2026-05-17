using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Reports;

public class RenameItemSimilarityGroupCommandHandler(IReportService reportService)
	: IRequestHandler<RenameItemSimilarityGroupCommand, int>
{
	public async ValueTask<int> Handle(RenameItemSimilarityGroupCommand request, CancellationToken cancellationToken)
	{
		return await reportService.RenameItemsAsync(
			request.ItemIds,
			request.NewDescription,
			cancellationToken);
	}
}
