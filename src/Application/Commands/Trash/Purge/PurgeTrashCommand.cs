using Application.Interfaces;

namespace Application.Commands.Trash.Purge;

public record PurgeTrashCommand : ICommand<bool>;
