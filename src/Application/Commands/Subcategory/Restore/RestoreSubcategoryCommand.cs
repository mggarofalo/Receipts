using Application.Interfaces;

namespace Application.Commands.Subcategory.Restore;

public record RestoreSubcategoryCommand(Guid Id) : ICommand<bool>;
