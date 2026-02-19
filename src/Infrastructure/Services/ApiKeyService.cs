using Application.Interfaces.Services;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class ApiKeyService(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IApiKeyService
{
	public async Task<string> CreateApiKeyAsync(string userId, string name, DateTimeOffset? expiresAt, CancellationToken cancellationToken = default)
	{
		string rawKey = GenerateRawKey();
		string keyHash = HashKey(rawKey);

		await using ApplicationDbContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		ApiKeyEntity entity = new()
		{
			Id = Guid.NewGuid(),
			Name = name,
			KeyHash = keyHash,
			UserId = userId,
			CreatedAt = DateTimeOffset.UtcNow,
			ExpiresAt = expiresAt,
			IsRevoked = false,
		};

		context.ApiKeys.Add(entity);
		await context.SaveChangesAsync(cancellationToken);
		return rawKey;
	}

	public async Task<IReadOnlyList<ApiKeyInfo>> GetApiKeysForUserAsync(string userId, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		return await context.ApiKeys
			.Where(k => k.UserId == userId && !k.IsRevoked)
			.Select(k => new ApiKeyInfo(k.Id, k.Name, k.CreatedAt, k.LastUsedAt, k.ExpiresAt, k.IsRevoked))
			.ToListAsync(cancellationToken);
	}

	public async Task RevokeApiKeyAsync(Guid id, string userId, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		ApiKeyEntity key = await context.ApiKeys
			.FirstOrDefaultAsync(k => k.Id == id && k.UserId == userId, cancellationToken)
			?? throw new KeyNotFoundException($"API key {id} not found for user {userId}.");

		key.IsRevoked = true;
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<string?> GetUserIdByApiKeyAsync(string rawKey, CancellationToken cancellationToken = default)
	{
		string keyHash = HashKey(rawKey);

		await using ApplicationDbContext context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
		ApiKeyEntity? key = await context.ApiKeys
			.FirstOrDefaultAsync(
				k => k.KeyHash == keyHash && !k.IsRevoked && (k.ExpiresAt == null || k.ExpiresAt > DateTimeOffset.UtcNow),
				cancellationToken);

		if (key is null)
		{
			return null;
		}

		key.LastUsedAt = DateTimeOffset.UtcNow;
		await context.SaveChangesAsync(cancellationToken);
		return key.UserId;
	}

	private static string GenerateRawKey()
	{
		byte[] bytes = new byte[32];
		RandomNumberGenerator.Fill(bytes);
		return Convert.ToBase64String(bytes);
	}

	private static string HashKey(string rawKey)
	{
		byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
		return Convert.ToHexString(bytes).ToLowerInvariant();
	}
}
