using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Account.Restore;

public class RestoreAccountCommandHandler(IAccountService accountService) : IRequestHandler<RestoreAccountCommand, bool>
{
	public async Task<bool> Handle(RestoreAccountCommand request, CancellationToken cancellationToken)
	{
		return await accountService.RestoreAsync(request.Id, cancellationToken);
	}
}
