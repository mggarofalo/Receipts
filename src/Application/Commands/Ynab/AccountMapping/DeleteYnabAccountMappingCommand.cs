using Application.Interfaces;

namespace Application.Commands.Ynab.AccountMapping;

public record DeleteYnabAccountMappingCommand(Guid Id) : ICommand<Mediator.Unit>;
