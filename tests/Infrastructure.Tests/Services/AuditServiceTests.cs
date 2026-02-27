using Application.Interfaces.Services;
using FluentAssertions;
using Infrastructure.Entities.Audit;
using Infrastructure.Services;
using Infrastructure.Tests.Helpers;
using Infrastructure.Tests.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Services;

public class AuditServiceTests
{
	[Fact]
	public async Task GetByEntityAsync_ReturnsLogsForSpecificEntity()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		string entityId = Guid.NewGuid().ToString();
		AuditLogEntity targetLog = AuditLogEntityGenerator.Generate(entityId: entityId, entityType: "Account");
		AuditLogEntity otherLog = AuditLogEntityGenerator.Generate(entityId: Guid.NewGuid().ToString(), entityType: "Account");

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddRangeAsync(targetLog, otherLog);
			await context.SaveChangesAsync();
		}

		AuditService service = new(contextFactory);

		// Act
		List<AuditLogDto> results = await service.GetByEntityAsync("Account", entityId);

		// Assert
		results.Should().HaveCount(1);
		results[0].EntityId.Should().Be(entityId);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByEntityAsync_NoLogs_ReturnsEmptyList()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuditService service = new(contextFactory);

		// Act
		List<AuditLogDto> results = await service.GetByEntityAsync("Account", Guid.NewGuid().ToString());

		// Assert
		results.Should().BeEmpty();

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetRecentAsync_ReturnsSpecifiedCount_OrderedByChangedAtDesc()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		AuditLogEntity oldLog = AuditLogEntityGenerator.Generate();
		oldLog.ChangedAt = DateTimeOffset.UtcNow.AddHours(-2);

		AuditLogEntity midLog = AuditLogEntityGenerator.Generate();
		midLog.ChangedAt = DateTimeOffset.UtcNow.AddHours(-1);

		AuditLogEntity newLog = AuditLogEntityGenerator.Generate();
		newLog.ChangedAt = DateTimeOffset.UtcNow;

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddRangeAsync(oldLog, midLog, newLog);
			await context.SaveChangesAsync();
		}

		AuditService service = new(contextFactory);

		// Act
		List<AuditLogDto> results = await service.GetRecentAsync(2);

		// Assert
		results.Should().HaveCount(2);
		results[0].ChangedAt.Should().BeOnOrAfter(results[1].ChangedAt);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByUserAsync_ReturnsLogsForSpecificUser()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		string userId = "target-user";
		AuditLogEntity targetLog = AuditLogEntityGenerator.Generate(changedByUserId: userId);
		AuditLogEntity otherLog = AuditLogEntityGenerator.Generate(changedByUserId: "other-user");

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddRangeAsync(targetLog, otherLog);
			await context.SaveChangesAsync();
		}

		AuditService service = new(contextFactory);

		// Act
		List<AuditLogDto> results = await service.GetByUserAsync(userId);

		// Assert
		results.Should().HaveCount(1);
		results[0].ChangedByUserId.Should().Be(userId);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByApiKeyAsync_ReturnsLogsForSpecificApiKey()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		Guid apiKeyId = Guid.NewGuid();
		AuditLogEntity targetLog = AuditLogEntityGenerator.Generate(changedByApiKeyId: apiKeyId);
		AuditLogEntity otherLog = AuditLogEntityGenerator.Generate(changedByApiKeyId: Guid.NewGuid());

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddRangeAsync(targetLog, otherLog);
			await context.SaveChangesAsync();
		}

		AuditService service = new(contextFactory);

		// Act
		List<AuditLogDto> results = await service.GetByApiKeyAsync(apiKeyId);

		// Assert
		results.Should().HaveCount(1);
		results[0].ChangedByApiKeyId.Should().Be(apiKeyId);

		contextFactory.ResetDatabase();
	}
}
