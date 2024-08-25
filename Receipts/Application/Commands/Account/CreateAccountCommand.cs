using Application.Common;
using Application.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Commands.Account;

public record CreateAccountCommand(string AccountCode, string Name, bool IsActive) : ICommand<Guid>;

public class CreateAccountCommandHandler(IAccountRepository accountRepository, IMapper mapper) : IRequestHandler<CreateAccountCommand, Guid>
{
	private readonly IAccountRepository _accountRepository = accountRepository;
	private readonly IMapper _mapper = mapper;

	public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
	{
		Domain.Core.Account account = _mapper.Map<Domain.Core.Account>(request);
		Domain.Core.Account createdEntity = await _accountRepository.CreateAsync(account, cancellationToken);
		await _accountRepository.SaveChangesAsync(cancellationToken);
		return createdEntity.Id!.Value;
	}
}