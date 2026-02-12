using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class AccountRepositoryTests
{
	private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsAccount()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		AccountEntity entity = AccountEntityGenerator.Generate();
		await context.Accounts.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_contextFactory);

		// Act
		AccountEntity? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeEquivalentTo(entity);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		AccountRepository repository = new(_contextFactory);

		// Act
		AccountEntity? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByTransactionIdAsync_ExistingTransactionId_ReturnsCorrectAccount()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		AccountEntity accountEntity = AccountEntityGenerator.Generate();
		await context.Accounts.AddAsync(accountEntity);
		TransactionEntity transactionEntity = TransactionEntityGenerator.Generate(accountId: accountEntity.Id);
		await context.Transactions.AddAsync(transactionEntity);
		await context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_contextFactory);

		// Act
		AccountEntity? actual = await repository.GetByTransactionIdAsync(transactionEntity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeEquivalentTo(accountEntity);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByTransactionIdAsync_NonExistingTransactionId_ReturnsNull()
	{
		// Arrange
		AccountRepository repository = new(_contextFactory);

		// Act
		AccountEntity? result = await repository.GetByTransactionIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllAccounts()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(2);
		await context.Accounts.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_contextFactory);

		// Act
		List<AccountEntity> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		actual.Should().BeEquivalentTo(entities);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CreateAsync_ValidAccounts_ReturnsCreatedAccounts()
	{
		// Arrange
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(2);
		entities.ForEach(e => e.Id = Guid.Empty);
		AccountRepository repository = new(_contextFactory);

		// Act
		List<AccountEntity> actual = await repository.CreateAsync(entities, CancellationToken.None);

		// Assert
		Assert.All(actual, a =>
		{
			Assert.NotEqual(Guid.Empty, a.Id);
		});

		actual.Should().BeEquivalentTo(entities, opt => opt.Excluding(x => x.Id));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task UpdateAsync_ValidAccount_UpdatesAccount()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(2);
		await context.Accounts.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_contextFactory);

		// Modify account
		entities.ForEach(e =>
		{
			e.Name = "Updated " + e.Name;
			e.IsActive = !e.IsActive;
		});

		// Act
		await repository.UpdateAsync(entities, CancellationToken.None);

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		List<AccountEntity> updatedEntities = await verifyContext.Accounts.ToListAsync();

		// Assert
		updatedEntities.Should().BeEquivalentTo(entities);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesAccounts()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);
		await context.Accounts.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		List<Guid> idsToDelete = entities.Take(2).Select(e => e.Id).ToList();
		AccountRepository repository = new(_contextFactory);

		// Act
		await repository.DeleteAsync(idsToDelete, CancellationToken.None);

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		List<AccountEntity> remainingEntities = await verifyContext.Accounts.ToListAsync();

		// Assert
		Assert.Single(remainingEntities);
		Assert.DoesNotContain(remainingEntities, e => idsToDelete.Contains(e.Id));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task ExistsAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		AccountEntity entity = AccountEntityGenerator.Generate();
		await context.Accounts.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_contextFactory);

		// Act
		bool result = await repository.ExistsAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.True(result);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task ExistsAsync_NonExistingId_ReturnsFalse()
	{
		// Arrange
		AccountRepository repository = new(_contextFactory);

		// Act
		bool result = await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.False(result);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetCountAsync_ReturnsCorrectCount()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);
		await context.Accounts.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_contextFactory);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		count.Should().Be(entities.Count);

		_contextFactory.ResetDatabase();
	}
}