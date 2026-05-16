using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Ynab.AccountMapping;

public class DeleteYnabAccountMappingCommandHandler(
	IYnabAccountMappingService accountMappingService) : IRequestHandler<DeleteYnabAccountMappingCommand, Unit>
{
	public async ValueTask<Unit> Handle(DeleteYnabAccountMappingCommand request, CancellationToken cancellationToken)
	{
		await accountMappingService.DeleteAsync(request.Id, cancellationToken);
		return Unit.Value;
	}
}
