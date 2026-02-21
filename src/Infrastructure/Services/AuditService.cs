using Application.Interfaces.Services;
using Infrastructure.Entities.Audit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AuditService(IDbContextFactory<ApplicationDbContext> contextFactory) : IAuditService
{
	public async Task<List<AuditLogDto>> GetByEntityAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		return await context.AuditLogs
			.Where(a => a.EntityType == entityType && a.EntityId == entityId)
			.OrderByDescending(a => a.ChangedAt)
			.Select(a => ToDto(a))
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AuditLogDto>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		return await context.AuditLogs
			.OrderByDescending(a => a.ChangedAt)
			.Take(count)
			.Select(a => ToDto(a))
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AuditLogDto>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		return await context.AuditLogs
			.Where(a => a.ChangedByUserId == userId)
			.OrderByDescending(a => a.ChangedAt)
			.Select(a => ToDto(a))
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AuditLogDto>> GetByApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		return await context.AuditLogs
			.Where(a => a.ChangedByApiKeyId == apiKeyId)
			.OrderByDescending(a => a.ChangedAt)
			.Select(a => ToDto(a))
			.ToListAsync(cancellationToken);
	}

	private static AuditLogDto ToDto(AuditLogEntity entity) => new(
		entity.Id,
		entity.EntityType,
		entity.EntityId,
		entity.Action.ToString(),
		entity.ChangesJson,
		entity.ChangedByUserId,
		entity.ChangedByApiKeyId,
		entity.ChangedAt,
		entity.IpAddress);
}
