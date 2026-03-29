using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Reports;

public class RenameItemSimilarityGroupCommandHandler(IReportService reportService)
	: IRequestHandler<RenameItemSimilarityGroupCommand, int>
{
	public async Task<int> Handle(RenameItemSimilarityGroupCommand request, CancellationToken cancellationToken)
	{
		return await reportService.RenameItemsAsync(
			request.ItemIds,
			request.NewDescription,
			cancellationToken);
	}
}
