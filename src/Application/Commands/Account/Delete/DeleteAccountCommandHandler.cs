using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Account.Delete;

public class DeleteAccountCommandHandler(IAccountService accountService) : IRequestHandler<DeleteAccountCommand, bool>
{
	public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
	{
		await accountService.DeleteAsync([.. request.Ids], cancellationToken);
		return true;
	}
}