using Application.Models;

namespace Application.Interfaces.Services;

public interface IAuthAuditService
{
	Task LogAsync(AuthAuditEntryDto entry, CancellationToken cancellationToken = default);
	Task<PagedResult<AuthAuditEntryDto>> GetMyAuditLogAsync(string userId, int offset, int limit, SortParams sort, CancellationToken cancellationToken = default);
	Task<PagedResult<AuthAuditEntryDto>> GetRecentAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken = default);
	Task<PagedResult<AuthAuditEntryDto>> GetFailedAttemptsAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken = default);
	Task<int> CleanupOldEntriesAsync(int retentionDays, CancellationToken cancellationToken = default);
}
