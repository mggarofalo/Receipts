using Application.Interfaces.Services;
using MediatR;

namespace Application.Commands.Account;

public class CreateAccountCommandHandler(IAccountService accountService) : IRequestHandler<CreateAccountCommand, List<Domain.Core.Account>>
{
	public async Task<List<Domain.Core.Account>> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
	{
		List<Domain.Core.Account> createdEntities = await accountService.CreateAsync([.. request.Accounts], cancellationToken);
		return createdEntities;
	}
}