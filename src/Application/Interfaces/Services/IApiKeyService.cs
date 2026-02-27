namespace Application.Interfaces.Services;

public record ApiKeyInfo(
	Guid Id,
	string Name,
	DateTimeOffset CreatedAt,
	DateTimeOffset? LastUsedAt,
	DateTimeOffset? ExpiresAt,
	bool IsRevoked);

public interface IApiKeyService
{
	Task<string> CreateApiKeyAsync(string userId, string name, DateTimeOffset? expiresAt, CancellationToken cancellationToken = default);
	Task<IReadOnlyList<ApiKeyInfo>> GetApiKeysForUserAsync(string userId, CancellationToken cancellationToken = default);
	Task RevokeApiKeyAsync(Guid id, string userId, CancellationToken cancellationToken = default);
	Task<string?> GetUserIdByApiKeyAsync(string rawKey, CancellationToken cancellationToken = default);
}
