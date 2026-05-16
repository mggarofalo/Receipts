using Application.Interfaces.Services;
using Application.Models.Ynab;
using Mediator;

namespace Application.Queries.Core.Ynab;

public class GetReceiptYnabSyncStatusesQueryHandler(IYnabSyncRecordService syncRecordService) : IRequestHandler<GetReceiptYnabSyncStatusesQuery, List<ReceiptYnabSyncStatusDto>>
{
	public async ValueTask<List<ReceiptYnabSyncStatusDto>> Handle(GetReceiptYnabSyncStatusesQuery request, CancellationToken cancellationToken)
	{
		return await syncRecordService.GetSyncStatusesByReceiptIdsAsync(request.ReceiptIds, cancellationToken);
	}
}
