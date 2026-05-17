using Application.Interfaces.Services;
using Application.Models.Ynab;
using Mediator;

namespace Application.Commands.Ynab.SyncRecord;

public class CreateYnabSyncRecordCommandHandler(IYnabSyncRecordService syncRecordService) : IRequestHandler<CreateYnabSyncRecordCommand, YnabSyncRecordDto>
{
	public async ValueTask<YnabSyncRecordDto> Handle(CreateYnabSyncRecordCommand request, CancellationToken cancellationToken)
	{
		return await syncRecordService.CreateAsync(request.LocalTransactionId, request.YnabBudgetId, request.SyncType, cancellationToken);
	}
}
