using Application.Interfaces.Services;
using Infrastructure.Entities.Audit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class AuthAuditService(IDbContextFactory<ApplicationDbContext> contextFactory) : IAuthAuditService
{
	public async Task LogAsync(AuthAuditEntryDto entry, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		AuthAuditLogEntity entity = new()
		{
			Id = entry.Id == Guid.Empty ? Guid.NewGuid() : entry.Id,
			EventType = Enum.Parse<AuthEventType>(entry.EventType),
			UserId = entry.UserId,
			ApiKeyId = entry.ApiKeyId,
			Username = entry.Username,
			Success = entry.Success,
			FailureReason = entry.FailureReason,
			IpAddress = entry.IpAddress,
			UserAgent = entry.UserAgent,
			Timestamp = entry.Timestamp,
			MetadataJson = entry.MetadataJson,
		};

		context.AuthAuditLogs.Add(entity);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<List<AuthAuditEntryDto>> GetMyAuditLogAsync(string userId, int count = 50, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		return await context.AuthAuditLogs
			.Where(a => a.UserId == userId)
			.OrderByDescending(a => a.Timestamp)
			.Take(count)
			.Select(a => ToDto(a))
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AuthAuditEntryDto>> GetRecentAsync(int count = 50, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		return await context.AuthAuditLogs
			.OrderByDescending(a => a.Timestamp)
			.Take(count)
			.Select(a => ToDto(a))
			.ToListAsync(cancellationToken);
	}

	public async Task<List<AuthAuditEntryDto>> GetFailedAttemptsAsync(int count = 50, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		return await context.AuthAuditLogs
			.Where(a => !a.Success)
			.OrderByDescending(a => a.Timestamp)
			.Take(count)
			.Select(a => ToDto(a))
			.ToListAsync(cancellationToken);
	}

	public async Task<int> CleanupOldEntriesAsync(int retentionDays, CancellationToken cancellationToken = default)
	{
		await using ApplicationDbContext context = await contextFactory.CreateDbContextAsync(cancellationToken);

		DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddDays(-retentionDays);

		return await context.AuthAuditLogs
			.Where(a => a.Timestamp < cutoff)
			.ExecuteDeleteAsync(cancellationToken);
	}

	private static AuthAuditEntryDto ToDto(AuthAuditLogEntity entity) => new(
		entity.Id,
		entity.EventType.ToString(),
		entity.UserId,
		entity.ApiKeyId,
		entity.Username,
		entity.Success,
		entity.FailureReason,
		entity.IpAddress,
		entity.UserAgent,
		entity.Timestamp,
		entity.MetadataJson);
}
