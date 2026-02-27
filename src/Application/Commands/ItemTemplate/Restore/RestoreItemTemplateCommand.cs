using Application.Interfaces;

namespace Application.Commands.ItemTemplate.Restore;

public record RestoreItemTemplateCommand(Guid Id) : ICommand<bool>;
