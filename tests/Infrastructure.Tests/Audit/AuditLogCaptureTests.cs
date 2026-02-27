using FluentAssertions;
using Infrastructure.Entities.Audit;
using Infrastructure.Entities.Core;
using Infrastructure.Tests.Helpers;
using Infrastructure.Tests.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Audit;

public class AuditLogCaptureTests
{
	[Fact]
	public async Task SaveChanges_CreateEntity_ProducesCreateAuditLog()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AccountEntity entity = AccountEntityGenerator.Generate();

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().HaveCount(1);

			AuditLogEntity log = auditLogs[0];
			log.Action.Should().Be(AuditAction.Create);
			log.EntityType.Should().Be("Account");

			List<FieldChange> changes = log.GetChanges();
			changes.Should().NotBeEmpty();
			changes.Should().AllSatisfy(c => c.OldValue.Should().BeNull());
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SaveChanges_UpdateEntity_ProducesUpdateAuditLogWithFieldChanges()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AccountEntity entity = AccountEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			account.Name = "Updated Name";
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs
				.Where(a => a.Action == AuditAction.Update)
				.ToListAsync();
			auditLogs.Should().HaveCount(1);

			AuditLogEntity log = auditLogs[0];
			List<FieldChange> changes = log.GetChanges();
			FieldChange nameChange = changes.Should().ContainSingle(c => c.FieldName == "Name").Subject;
			nameChange.OldValue.Should().Be("Test Account");
			nameChange.NewValue.Should().Be("Updated Name");
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SaveChanges_UpdateEntity_OnlyLogsChangedFields()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AccountEntity entity = AccountEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			account.Name = "Only Name Changed";
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs
				.Where(a => a.Action == AuditAction.Update)
				.ToListAsync();
			auditLogs.Should().HaveCount(1);

			List<FieldChange> changes = auditLogs[0].GetChanges();
			changes.Should().ContainSingle(c => c.FieldName == "Name");
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SaveChanges_DeleteEntity_ProducesSoftDeleteAuditLog()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AccountEntity entity = AccountEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			context.Accounts.Remove(account);
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs
				.Where(a => a.Action == AuditAction.Delete)
				.ToListAsync();
			auditLogs.Should().HaveCount(1);
			auditLogs[0].EntityType.Should().Be("Account");
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SaveChanges_RestoreEntity_ProducesRestoreAuditLog()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AccountEntity entity = AccountEntityGenerator.Generate();

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts.FirstAsync();
			context.Accounts.Remove(account);
			await context.SaveChangesAsync();
		}

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			AccountEntity account = await context.Accounts
				.IgnoreQueryFilters()
				.FirstAsync(a => a.Id == entity.Id);
			account.DeletedAt = null;
			account.DeletedByUserId = null;
			account.DeletedByApiKeyId = null;
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs
				.Where(a => a.Action == AuditAction.Restore)
				.ToListAsync();
			auditLogs.Should().HaveCount(1);
			auditLogs[0].EntityType.Should().Be("Account");
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SaveChanges_AuditLogEntity_NotSelfAudited()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AuditLogEntity auditLog = AuditLogEntityGenerator.Generate();

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.AuditLogs.AddAsync(auditLog);
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().HaveCount(1);
			auditLogs[0].Id.Should().Be(auditLog.Id);
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SaveChanges_MultipleEntities_ProducesMultipleAuditLogs()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddRangeAsync(entities);
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs
				.Where(a => a.Action == AuditAction.Create)
				.ToListAsync();
			auditLogs.Should().HaveCount(3);
			auditLogs.Should().AllSatisfy(a => a.EntityType.Should().Be("Account"));
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SaveChanges_EntityWithUserContext_CapturesUserIdAndIpAddress()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor accessor) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		accessor.UserId = "test-user-123";
		accessor.IpAddress = "192.168.1.1";
		AccountEntity entity = AccountEntityGenerator.Generate();

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().HaveCount(1);
			auditLogs[0].ChangedByUserId.Should().Be("test-user-123");
			auditLogs[0].IpAddress.Should().Be("192.168.1.1");
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SaveChanges_EntityWithApiKeyContext_CapturesApiKeyId()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor accessor) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		Guid apiKeyId = Guid.NewGuid();
		accessor.ApiKeyId = apiKeyId;
		AccountEntity entity = AccountEntityGenerator.Generate();

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().HaveCount(1);
			auditLogs[0].ChangedByApiKeyId.Should().Be(apiKeyId);
		}

		contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task SaveChanges_CreateEntity_CapturesGeneratedId()
	{
		// Arrange
		(IDbContextFactory<ApplicationDbContext> contextFactory, MockCurrentUserAccessor _) = DbContextWithUserHelpers.CreateInMemoryContextFactoryWithUser();
		AccountEntity entity = AccountEntityGenerator.Generate();

		// Act
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			await context.Accounts.AddAsync(entity);
			await context.SaveChangesAsync();
		}

		// Assert
		await using (ApplicationDbContext context = contextFactory.CreateDbContext())
		{
			List<AuditLogEntity> auditLogs = await context.AuditLogs.ToListAsync();
			auditLogs.Should().HaveCount(1);

			AuditLogEntity log = auditLogs[0];
			log.EntityId.Should().NotBeNullOrEmpty();
			log.EntityId.Should().NotBe(Guid.Empty.ToString());
			Guid.Parse(log.EntityId).Should().Be(entity.Id);
		}

		contextFactory.ResetDatabase();
	}
}
