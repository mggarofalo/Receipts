using Application.Interfaces;

namespace Application.Commands.Category.Restore;

public record RestoreCategoryCommand(Guid Id) : ICommand<bool>;
