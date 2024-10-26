using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Account;

public class UpdateAccountCommandHandler(IAccountService accountService) : IRequestHandler<UpdateAccountCommand, bool>
{
	public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
	{
		await accountService.UpdateAsync([.. request.Accounts], cancellationToken);
		return true;
	}
}