using Application.Interfaces;

namespace Application.Commands.Adjustment.Restore;

public record RestoreAdjustmentCommand(Guid Id) : ICommand<bool>;
