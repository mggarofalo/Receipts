namespace Application.Interfaces.Services;

public interface IAuditService
{
	Task<List<AuditLogDto>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
	Task<List<AuditLogDto>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
	Task<List<AuditLogDto>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
	Task<List<AuditLogDto>> GetByApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default);
}
