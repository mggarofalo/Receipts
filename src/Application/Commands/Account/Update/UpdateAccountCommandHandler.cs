using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Account.Update;

public class UpdateAccountCommandHandler(IAccountService accountService) : IRequestHandler<UpdateAccountCommand, bool>
{
	public async Task<bool> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
	{
		foreach (Domain.Core.Account account in request.Accounts)
		{
			bool exists = await accountService.ExistsAsync(account.Id, cancellationToken);
			if (!exists)
			{
				return false;
			}
		}

		await accountService.UpdateAsync([.. request.Accounts], cancellationToken);
		return true;
	}
}
