using Application.Models;

namespace Application.Interfaces.Services;

public interface IAuditService
{
	Task<PagedResult<AuditLogDto>> GetByEntityAsync(string entityType, string entityId, int offset, int limit, SortParams sort, CancellationToken cancellationToken = default);
	Task<PagedResult<AuditLogDto>> GetRecentAsync(int offset, int limit, SortParams sort, string? entityType = null, string? action = null, string? search = null, DateTimeOffset? dateFrom = null, DateTimeOffset? dateTo = null, CancellationToken cancellationToken = default);
	Task<PagedResult<AuditLogDto>> GetByUserAsync(string userId, int offset, int limit, SortParams sort, CancellationToken cancellationToken = default);
	Task<PagedResult<AuditLogDto>> GetByApiKeyAsync(Guid apiKeyId, int offset, int limit, SortParams sort, CancellationToken cancellationToken = default);
}
