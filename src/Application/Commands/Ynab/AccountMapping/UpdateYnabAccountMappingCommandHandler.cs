using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Ynab.AccountMapping;

public class UpdateYnabAccountMappingCommandHandler(
	IYnabAccountMappingService accountMappingService) : IRequestHandler<UpdateYnabAccountMappingCommand, Unit>
{
	public async ValueTask<Unit> Handle(UpdateYnabAccountMappingCommand request, CancellationToken cancellationToken)
	{
		await accountMappingService.UpdateAsync(
			request.Id,
			request.YnabAccountId,
			request.YnabAccountName,
			request.YnabBudgetId,
			cancellationToken);
		return Unit.Value;
	}
}
