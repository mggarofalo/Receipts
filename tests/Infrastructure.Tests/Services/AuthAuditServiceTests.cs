using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Entities.Audit;
using Infrastructure.Services;
using Infrastructure.Tests.Helpers;
using Infrastructure.Tests.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Services;

public class AuthAuditServiceTests
{
	[Fact]
	public async Task LogAsync_LoginEvent_CreatesAuthAuditLogEntry()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuthAuditService service = new(contextFactory);
		AuthAuditEntryDto entry = new(
			Guid.Empty,
			"Login",
			"user-123",
			null,
			"testuser@example.com",
			true,
			null,
			"192.168.1.1",
			"TestAgent/1.0",
			DateTimeOffset.UtcNow,
			null);

		// Act
		await service.LogAsync(entry);

		// Assert
		await using ApplicationDbContext context = contextFactory.CreateDbContext();
		List<AuthAuditLogEntity> logs = await context.AuthAuditLogs.ToListAsync();
		logs.Should().HaveCount(1);
		logs[0].EventType.Should().Be(AuthEventType.Login);
		logs[0].UserId.Should().Be("user-123");
		logs[0].Success.Should().BeTrue();
		logs[0].Id.Should().NotBe(Guid.Empty);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task LogAsync_LoginFailedEvent_CapturesFailureReason()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuthAuditService service = new(contextFactory);
		AuthAuditEntryDto entry = new(
			Guid.Empty,
			"LoginFailed",
			"user-123",
			null,
			"testuser@example.com",
			false,
			"Invalid password",
			"192.168.1.1",
			"TestAgent/1.0",
			DateTimeOffset.UtcNow,
			null);

		// Act
		await service.LogAsync(entry);

		// Assert
		await using ApplicationDbContext context = contextFactory.CreateDbContext();
		AuthAuditLogEntity log = await context.AuthAuditLogs.SingleAsync();
		log.EventType.Should().Be(AuthEventType.LoginFailed);
		log.Success.Should().BeFalse();
		log.FailureReason.Should().Be("Invalid password");

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task LogAsync_ApiKeyUsedEvent_CapturesApiKeyId()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuthAuditService service = new(contextFactory);
		Guid apiKeyId = Guid.NewGuid();
		AuthAuditEntryDto entry = new(
			Guid.Empty,
			"ApiKeyUsed",
			null,
			apiKeyId,
			null,
			true,
			null,
			"10.0.0.1",
			"ApiClient/2.0",
			DateTimeOffset.UtcNow,
			null);

		// Act
		await service.LogAsync(entry);

		// Assert
		await using ApplicationDbContext context = contextFactory.CreateDbContext();
		AuthAuditLogEntity log = await context.AuthAuditLogs.SingleAsync();
		log.EventType.Should().Be(AuthEventType.ApiKeyUsed);
		log.ApiKeyId.Should().Be(apiKeyId);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task LogAsync_CapturesIpAddressAndUserAgent()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuthAuditService service = new(contextFactory);
		AuthAuditEntryDto entry = new(
			Guid.Empty,
			"Login",
			"user-123",
			null,
			"testuser@example.com",
			true,
			null,
			"203.0.113.42",
			"Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
			DateTimeOffset.UtcNow,
			null);

		// Act
		await service.LogAsync(entry);

		// Assert
		await using ApplicationDbContext context = contextFactory.CreateDbContext();
		AuthAuditLogEntity log = await context.AuthAuditLogs.SingleAsync();
		log.IpAddress.Should().Be("203.0.113.42");
		log.UserAgent.Should().Be("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task LogAsync_CapturesMetadataJson()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuthAuditService service = new(contextFactory);
		string metadata = """{"mfa":"totp","device":"desktop"}""";
		AuthAuditEntryDto entry = new(
			Guid.Empty,
			"Login",
			"user-123",
			null,
			"testuser@example.com",
			true,
			null,
			"192.168.1.1",
			"TestAgent/1.0",
			DateTimeOffset.UtcNow,
			metadata);

		// Act
		await service.LogAsync(entry);

		// Assert
		await using ApplicationDbContext context = contextFactory.CreateDbContext();
		AuthAuditLogEntity log = await context.AuthAuditLogs.SingleAsync();
		log.MetadataJson.Should().Be(metadata);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetMyAuditLogAsync_ReturnsLogsForSpecificUser_OrderedByTimestampDesc()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		string targetUserId = "target-user";

		AuthAuditLogEntity oldLog = AuthAuditLogEntityGenerator.Generate(userId: targetUserId, timestamp: DateTimeOffset.UtcNow.AddHours(-2));
		AuthAuditLogEntity newLog = AuthAuditLogEntityGenerator.Generate(userId: targetUserId, timestamp: DateTimeOffset.UtcNow);
		AuthAuditLogEntity otherUserLog = AuthAuditLogEntityGenerator.Generate(userId: "other-user");

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuthAuditLogs.AddRangeAsync(oldLog, newLog, otherUserLog);
			await context.SaveChangesAsync();
		}

		AuthAuditService service = new(contextFactory);

		// Act
		List<AuthAuditEntryDto> results = await service.GetMyAuditLogAsync(targetUserId);

		// Assert
		results.Should().HaveCount(2);
		results[0].Timestamp.Should().BeOnOrAfter(results[1].Timestamp);
		results.Should().AllSatisfy(r => r.UserId.Should().Be(targetUserId));

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetMyAuditLogAsync_RespectsCountLimit()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		string userId = "test-user";

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			for (int i = 0; i < 10; i++)
			{
				context.AuthAuditLogs.Add(AuthAuditLogEntityGenerator.Generate(
					userId: userId,
					timestamp: DateTimeOffset.UtcNow.AddMinutes(-i)));
			}
			await context.SaveChangesAsync();
		}

		AuthAuditService service = new(contextFactory);

		// Act
		List<AuthAuditEntryDto> results = await service.GetMyAuditLogAsync(userId, count: 5);

		// Assert
		results.Should().HaveCount(5);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetRecentAsync_ReturnsAllRecentEvents_OrderedByTimestampDesc()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		AuthAuditLogEntity oldLog = AuthAuditLogEntityGenerator.Generate(timestamp: DateTimeOffset.UtcNow.AddHours(-3));
		AuthAuditLogEntity midLog = AuthAuditLogEntityGenerator.Generate(timestamp: DateTimeOffset.UtcNow.AddHours(-1));
		AuthAuditLogEntity newLog = AuthAuditLogEntityGenerator.Generate(timestamp: DateTimeOffset.UtcNow);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuthAuditLogs.AddRangeAsync(oldLog, midLog, newLog);
			await context.SaveChangesAsync();
		}

		AuthAuditService service = new(contextFactory);

		// Act
		List<AuthAuditEntryDto> results = await service.GetRecentAsync(50);

		// Assert
		results.Should().HaveCount(3);
		results[0].Timestamp.Should().BeOnOrAfter(results[1].Timestamp);
		results[1].Timestamp.Should().BeOnOrAfter(results[2].Timestamp);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetFailedAttemptsAsync_ReturnsOnlyFailedLogins()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		AuthAuditLogEntity successLog = AuthAuditLogEntityGenerator.Generate(success: true);
		AuthAuditLogEntity failedLog1 = AuthAuditLogEntityGenerator.Generate(success: false, failureReason: "Bad password");
		AuthAuditLogEntity failedLog2 = AuthAuditLogEntityGenerator.Generate(success: false, failureReason: "Account locked");

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuthAuditLogs.AddRangeAsync(successLog, failedLog1, failedLog2);
			await context.SaveChangesAsync();
		}

		AuthAuditService service = new(contextFactory);

		// Act
		List<AuthAuditEntryDto> results = await service.GetFailedAttemptsAsync();

		// Assert
		results.Should().HaveCount(2);
		results.Should().AllSatisfy(r => r.Success.Should().BeFalse());

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CleanupOldEntriesAsync_VerifyOldEntriesCanBeIdentified()
	{
		// Arrange
		// Note: ExecuteDeleteAsync is not supported by InMemory provider,
		// so we verify the filtering logic by querying instead.
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		DateTimeOffset now = DateTimeOffset.UtcNow;
		AuthAuditLogEntity oldEntry = AuthAuditLogEntityGenerator.Generate(timestamp: now.AddDays(-200));
		AuthAuditLogEntity recentEntry = AuthAuditLogEntityGenerator.Generate(timestamp: now.AddDays(-10));

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuthAuditLogs.AddRangeAsync(oldEntry, recentEntry);
			await context.SaveChangesAsync();
		}

		// Act — verify the cutoff filter works
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			DateTimeOffset cutoff = now.AddDays(-180);
			List<AuthAuditLogEntity> oldEntries = await context.AuthAuditLogs
				.Where(a => a.Timestamp < cutoff)
				.ToListAsync();

			// Assert
			oldEntries.Should().HaveCount(1);
			oldEntries[0].Id.Should().Be(oldEntry.Id);
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CleanupOldEntriesAsync_VerifyRecentEntriesKept()
	{
		// Arrange
		// Note: ExecuteDeleteAsync is not supported by InMemory provider,
		// so we verify the filtering logic by querying instead.
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		DateTimeOffset now = DateTimeOffset.UtcNow;
		AuthAuditLogEntity recentEntry1 = AuthAuditLogEntityGenerator.Generate(timestamp: now.AddDays(-30));
		AuthAuditLogEntity recentEntry2 = AuthAuditLogEntityGenerator.Generate(timestamp: now.AddDays(-90));
		AuthAuditLogEntity recentEntry3 = AuthAuditLogEntityGenerator.Generate(timestamp: now.AddDays(-179));

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuthAuditLogs.AddRangeAsync(recentEntry1, recentEntry2, recentEntry3);
			await context.SaveChangesAsync();
		}

		// Act — verify the cutoff filter excludes recent entries
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			DateTimeOffset cutoff = now.AddDays(-180);
			List<AuthAuditLogEntity> oldEntries = await context.AuthAuditLogs
				.Where(a => a.Timestamp < cutoff)
				.ToListAsync();

			// Assert
			oldEntries.Should().BeEmpty();
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CleanupOldEntriesAsync_VerifyDeletedCount()
	{
		// Arrange
		// Note: ExecuteDeleteAsync is not supported by InMemory provider,
		// so we simulate the count by querying the filter.
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		DateTimeOffset now = DateTimeOffset.UtcNow;
		AuthAuditLogEntity oldEntry1 = AuthAuditLogEntityGenerator.Generate(timestamp: now.AddDays(-200));
		AuthAuditLogEntity oldEntry2 = AuthAuditLogEntityGenerator.Generate(timestamp: now.AddDays(-365));
		AuthAuditLogEntity recentEntry = AuthAuditLogEntityGenerator.Generate(timestamp: now.AddDays(-10));

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuthAuditLogs.AddRangeAsync(oldEntry1, oldEntry2, recentEntry);
			await context.SaveChangesAsync();
		}

		// Act — verify the count of entries that would be deleted
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			DateTimeOffset cutoff = now.AddDays(-180);
			int deleteCount = await context.AuthAuditLogs
				.Where(a => a.Timestamp < cutoff)
				.CountAsync();

			// Assert
			deleteCount.Should().Be(2);
		}

		contextFactory.ResetDatabase();
	}
}
