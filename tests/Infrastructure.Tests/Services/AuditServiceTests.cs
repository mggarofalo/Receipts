using Application.Interfaces.Services;
using Application.Models;
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
	private static readonly SortParams DefaultSort = SortParams.Default;

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
		PagedResult<AuditLogDto> results = await service.GetByEntityAsync("Account", entityId, 0, 50, DefaultSort);

		// Assert
		results.Data.Should().HaveCount(1);
		results.Data[0].EntityId.Should().Be(entityId);
		results.Total.Should().Be(1);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByEntityAsync_NoLogs_ReturnsEmptyResult()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuditService service = new(contextFactory);

		// Act
		PagedResult<AuditLogDto> results = await service.GetByEntityAsync("Account", Guid.NewGuid().ToString(), 0, 50, DefaultSort);

		// Assert
		results.Data.Should().BeEmpty();
		results.Total.Should().Be(0);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetRecentAsync_ReturnsPagedResults_OrderedByChangedAtDesc()
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
		PagedResult<AuditLogDto> results = await service.GetRecentAsync(0, 2, DefaultSort);

		// Assert
		results.Data.Should().HaveCount(2);
		results.Total.Should().Be(3);
		results.Data[0].ChangedAt.Should().BeOnOrAfter(results.Data[1].ChangedAt);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetRecentAsync_FiltersBy_EntityType()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		AuditLogEntity accountLog = AuditLogEntityGenerator.Generate(entityType: "Account");
		AuditLogEntity receiptLog = AuditLogEntityGenerator.Generate(entityType: "Receipt");

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddRangeAsync(accountLog, receiptLog);
			await context.SaveChangesAsync();
		}

		AuditService service = new(contextFactory);

		// Act
		PagedResult<AuditLogDto> result = await service.GetRecentAsync(0, 50, DefaultSort, entityType: "Account");

		// Assert
		result.Data.Should().HaveCount(1);
		result.Data[0].EntityType.Should().Be("Account");
		result.Total.Should().Be(1);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetRecentAsync_FiltersBy_Action()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		AuditLogEntity createLog = AuditLogEntityGenerator.Generate(action: AuditAction.Create);
		AuditLogEntity updateLog = AuditLogEntityGenerator.Generate(action: AuditAction.Update);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddRangeAsync(createLog, updateLog);
			await context.SaveChangesAsync();
		}

		AuditService service = new(contextFactory);

		// Act
		PagedResult<AuditLogDto> result = await service.GetRecentAsync(0, 50, DefaultSort, action: "Create");

		// Assert
		result.Data.Should().HaveCount(1);
		result.Data[0].Action.Should().Be("Create");

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetRecentAsync_FiltersBy_Search()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		AuditLogEntity matchLog = AuditLogEntityGenerator.Generate(entityId: "abc-123-def");
		AuditLogEntity otherLog = AuditLogEntityGenerator.Generate(entityId: "xyz-789-ghi");

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddRangeAsync(matchLog, otherLog);
			await context.SaveChangesAsync();
		}

		AuditService service = new(contextFactory);

		// Act
		PagedResult<AuditLogDto> result = await service.GetRecentAsync(0, 50, DefaultSort, search: "abc-123");

		// Assert
		result.Data.Should().HaveCount(1);
		result.Data[0].EntityId.Should().Contain("abc-123");

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetRecentAsync_FiltersBy_DateRange()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		AuditLogEntity oldLog = AuditLogEntityGenerator.Generate();
		oldLog.ChangedAt = DateTimeOffset.UtcNow.AddDays(-10);

		AuditLogEntity recentLog = AuditLogEntityGenerator.Generate();
		recentLog.ChangedAt = DateTimeOffset.UtcNow.AddDays(-1);

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddRangeAsync(oldLog, recentLog);
			await context.SaveChangesAsync();
		}

		AuditService service = new(contextFactory);

		// Act
		PagedResult<AuditLogDto> result = await service.GetRecentAsync(0, 50, DefaultSort, dateFrom: DateTimeOffset.UtcNow.AddDays(-3));

		// Assert
		result.Data.Should().HaveCount(1);
		result.Total.Should().Be(1);

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetRecentAsync_Paginates_WithOffset()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();

		for (int i = 0; i < 5; i++)
		{
			AuditLogEntity log = AuditLogEntityGenerator.Generate();
			log.ChangedAt = DateTimeOffset.UtcNow.AddMinutes(-i);

			await using ApplicationDbContext context = contextFactory.CreateDbContext();
			await context.AuditLogs.AddAsync(log);
			await context.SaveChangesAsync();
		}

		AuditService service = new(contextFactory);

		// Act
		PagedResult<AuditLogDto> results = await service.GetRecentAsync(2, 2, DefaultSort);

		// Assert
		results.Data.Should().HaveCount(2);
		results.Total.Should().Be(5);
		results.Offset.Should().Be(2);
		results.Limit.Should().Be(2);

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
		PagedResult<AuditLogDto> results = await service.GetByUserAsync(userId, 0, 50, DefaultSort);

		// Assert
		results.Data.Should().HaveCount(1);
		results.Data[0].ChangedByUserId.Should().Be(userId);
		results.Total.Should().Be(1);

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
		PagedResult<AuditLogDto> results = await service.GetByApiKeyAsync(apiKeyId, 0, 50, DefaultSort);

		// Assert
		results.Data.Should().HaveCount(1);
		results.Data[0].ChangedByApiKeyId.Should().Be(apiKeyId);
		results.Total.Should().Be(1);

		contextFactory.ResetDatabase();
	}
}
