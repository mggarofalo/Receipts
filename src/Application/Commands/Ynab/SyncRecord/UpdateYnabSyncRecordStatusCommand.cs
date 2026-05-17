using Application.Interfaces;
using Common;

namespace Application.Commands.Ynab.SyncRecord;

public record UpdateYnabSyncRecordStatusCommand(Guid Id, YnabSyncStatus Status, string? YnabTransactionId, string? LastError) : ICommand<Mediator.Unit>;
