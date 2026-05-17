using Application.Interfaces.Services;
using Mediator;

namespace Application.Commands.Account.Delete;

public class DeleteAccountCommandHandler(IAccountService accountService) : IRequestHandler<DeleteAccountCommand, bool>
{
	public async ValueTask<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
	{
		bool exists = await accountService.ExistsAsync(request.Id, cancellationToken);
		if (!exists)
		{
			return false;
		}

		await accountService.DeleteAsync(request.Id, cancellationToken);
		return true;
	}
}
