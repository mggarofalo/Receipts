namespace Application.Interfaces.Services;

public interface IAuthAuditService
{
	Task LogAsync(AuthAuditEntryDto entry, CancellationToken cancellationToken = default);
	Task<List<AuthAuditEntryDto>> GetMyAuditLogAsync(string userId, int count = 50, CancellationToken cancellationToken = default);
	Task<List<AuthAuditEntryDto>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default);
	Task<List<AuthAuditEntryDto>> GetFailedAttemptsAsync(int count = 50, CancellationToken cancellationToken = default);
	Task<int> CleanupOldEntriesAsync(int retentionDays, CancellationToken cancellationToken = default);
}
