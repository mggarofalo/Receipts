using Application.Interfaces;

namespace Application.Commands.Ynab.CategoryMapping;

public record DeleteYnabCategoryMappingCommand(Guid Id) : ICommand<Mediator.Unit>;
