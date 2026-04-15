using Application.Models;
using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class CardRepositoryTests
{
	private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsAccount()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		CardEntity entity = CardEntityGenerator.Generate();
		await context.Cards.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

		// Act
		CardEntity? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeEquivalentTo(entity);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		CardRepository repository = new(_contextFactory);

		// Act
		CardEntity? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByTransactionIdAsync_ExistingTransactionId_ReturnsCorrectAccount()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		AccountEntity parentAccount = AccountEntityGenerator.Generate();
		await context.Accounts.AddAsync(parentAccount);
		CardEntity accountEntity = CardEntityGenerator.Generate();
		accountEntity.AccountId = parentAccount.Id;
		await context.Cards.AddAsync(accountEntity);
		TransactionEntity transactionEntity = TransactionEntityGenerator.Generate(accountId: parentAccount.Id);
		await context.Transactions.AddAsync(transactionEntity);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

		// Act
		CardEntity? actual = await repository.GetByTransactionIdAsync(transactionEntity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeEquivalentTo(accountEntity, opt => opt.Excluding(x => x.ParentAccount));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByTransactionIdAsync_NonExistingTransactionId_ReturnsNull()
	{
		// Arrange
		CardRepository repository = new(_contextFactory);

		// Act
		CardEntity? result = await repository.GetByTransactionIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllAccounts()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<CardEntity> entities = CardEntityGenerator.GenerateList(2);
		await context.Cards.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

		// Act
		List<CardEntity> actual = await repository.GetAllAsync(0, 50, SortParams.Default, CancellationToken.None);

		// Assert
		actual.Should().BeEquivalentTo(entities);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetAllAsync_WithIsActiveTrue_ReturnsOnlyActiveAccounts()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		CardEntity activeAccount = CardEntityGenerator.Generate();
		activeAccount.IsActive = true;
		CardEntity inactiveAccount = CardEntityGenerator.Generate();
		inactiveAccount.IsActive = false;
		await context.Cards.AddRangeAsync([activeAccount, inactiveAccount]);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

		// Act
		List<CardEntity> actual = await repository.GetAllAsync(0, 50, SortParams.Default, CancellationToken.None, isActive: true);

		// Assert
		actual.Should().HaveCount(1);
		actual[0].Id.Should().Be(activeAccount.Id);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetAllAsync_WithIsActiveFalse_ReturnsOnlyInactiveAccounts()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		CardEntity activeAccount = CardEntityGenerator.Generate();
		activeAccount.IsActive = true;
		CardEntity inactiveAccount = CardEntityGenerator.Generate();
		inactiveAccount.IsActive = false;
		await context.Cards.AddRangeAsync([activeAccount, inactiveAccount]);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

		// Act
		List<CardEntity> actual = await repository.GetAllAsync(0, 50, SortParams.Default, CancellationToken.None, isActive: false);

		// Assert
		actual.Should().HaveCount(1);
		actual[0].Id.Should().Be(inactiveAccount.Id);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetAllAsync_WithIsActiveNull_ReturnsAllAccounts()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		CardEntity activeAccount = CardEntityGenerator.Generate();
		activeAccount.IsActive = true;
		CardEntity inactiveAccount = CardEntityGenerator.Generate();
		inactiveAccount.IsActive = false;
		await context.Cards.AddRangeAsync([activeAccount, inactiveAccount]);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

		// Act
		List<CardEntity> actual = await repository.GetAllAsync(0, 50, SortParams.Default, CancellationToken.None, isActive: null);

		// Assert
		actual.Should().HaveCount(2);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetCountAsync_WithIsActive_ReturnsFilteredCount()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		CardEntity activeAccount1 = CardEntityGenerator.Generate();
		activeAccount1.IsActive = true;
		CardEntity activeAccount2 = CardEntityGenerator.Generate();
		activeAccount2.IsActive = true;
		CardEntity inactiveAccount = CardEntityGenerator.Generate();
		inactiveAccount.IsActive = false;
		await context.Cards.AddRangeAsync([activeAccount1, activeAccount2, inactiveAccount]);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

		// Act
		int activeCount = await repository.GetCountAsync(CancellationToken.None, isActive: true);
		int inactiveCount = await repository.GetCountAsync(CancellationToken.None, isActive: false);
		int allCount = await repository.GetCountAsync(CancellationToken.None, isActive: null);

		// Assert
		activeCount.Should().Be(2);
		inactiveCount.Should().Be(1);
		allCount.Should().Be(3);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CreateAsync_ValidAccounts_ReturnsCreatedAccounts()
	{
		// Arrange
		List<CardEntity> entities = CardEntityGenerator.GenerateList(2);
		entities.ForEach(e => e.Id = Guid.Empty);
		CardRepository repository = new(_contextFactory);

		// Act
		List<CardEntity> actual = await repository.CreateAsync(entities, CancellationToken.None);

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
		List<CardEntity> entities = CardEntityGenerator.GenerateList(2);
		await context.Cards.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

		// Modify account
		entities.ForEach(e =>
		{
			e.Name = "Updated " + e.Name;
			e.IsActive = !e.IsActive;
		});

		// Act
		await repository.UpdateAsync(entities, CancellationToken.None);

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		List<CardEntity> updatedEntities = await verifyContext.Cards.ToListAsync();

		// Assert
		updatedEntities.Should().BeEquivalentTo(entities);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task ExistsAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		CardEntity entity = CardEntityGenerator.Generate();
		await context.Cards.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

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
		CardRepository repository = new(_contextFactory);

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
		List<CardEntity> entities = CardEntityGenerator.GenerateList(3);
		await context.Cards.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		CardRepository repository = new(_contextFactory);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		count.Should().Be(entities.Count);

		_contextFactory.ResetDatabase();
	}
}