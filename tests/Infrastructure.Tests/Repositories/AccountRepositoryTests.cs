using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class AccountRepositoryTests
{
	private readonly ApplicationDbContext _context = DbContextHelpers.CreateInMemoryContext();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsAccount()
	{
		// Arrange
		_context.ResetDatabase();

		AccountEntity entity = AccountEntityGenerator.Generate();
		await _context.Accounts.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_context);

		// Act
		AccountEntity? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(entity, actual);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		_context.ResetDatabase();

		AccountRepository repository = new(_context);

		// Act
		AccountEntity? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetByTransactionIdAsync_ExistingTransactionId_ReturnsCorrectAccount()
	{
		// Arrange
		_context.ResetDatabase();

		AccountEntity accountEntity = AccountEntityGenerator.Generate();
		await _context.Accounts.AddAsync(accountEntity);
		TransactionEntity transactionEntity = TransactionEntityGenerator.Generate(accountId: accountEntity.Id);
		await _context.Transactions.AddAsync(transactionEntity);
		await _context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_context);

		// Act
		AccountEntity? actual = await repository.GetByTransactionIdAsync(transactionEntity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(accountEntity, actual);
	}

	[Fact]
	public async Task GetByTransactionIdAsync_NonExistingTransactionId_ReturnsNull()
	{
		// Arrange
		_context.ResetDatabase();

		AccountRepository repository = new(_context);

		// Act
		AccountEntity? result = await repository.GetByTransactionIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllAccounts()
	{
		// Arrange
		_context.ResetDatabase();

		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(2);
		await _context.Accounts.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_context);

		// Act
		List<AccountEntity> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);
		Assert.Equal(entities, actual);
	}

	[Fact]
	public async Task CreateAsync_ValidAccounts_ReturnsCreatedAccounts()
	{
		// Arrange
		_context.ResetDatabase();

		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(2);
		entities.ForEach(e => e.Id = Guid.Empty);
		AccountRepository repository = new(_context);

		// Act
		List<AccountEntity> actual = await repository.CreateAsync(entities, CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);

		Assert.All(actual, a =>
		{
			Assert.NotEqual(Guid.Empty, a.Id);
		});

		Assert.Equal(entities, actual.Select(a =>
		{
			a.Id = Guid.Empty;
			return a;
		}));
	}

	[Fact]
	public async Task UpdateAsync_ValidAccount_UpdatesAccount()
	{
		// Arrange
		_context.ResetDatabase();

		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(2);
		await _context.Accounts.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_context);

		// Modify account
		entities.ForEach(e =>
		{
			e.Name = "Updated " + e.Name;
			e.IsActive = !e.IsActive;
		});

		// Act
		await repository.UpdateAsync(entities, CancellationToken.None);
		List<AccountEntity> updatedEntities = await _context.Accounts.ToListAsync();

		// Assert
		Assert.Equal(entities.Count, updatedEntities.Count);

		foreach (AccountEntity expectedAccount in entities)
		{
			AccountEntity? updatedEntity = updatedEntities.FirstOrDefault(e => e.Id == expectedAccount.Id);
			Assert.NotNull(updatedEntity);
			Assert.Equal(expectedAccount.Name, updatedEntity.Name);
			Assert.Equal(expectedAccount.IsActive, updatedEntity.IsActive);
		}
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesAccounts()
	{
		// Arrange
		_context.ResetDatabase();

		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);
		await _context.Accounts.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<Guid> idsToDelete = entities.Take(2).Select(e => e.Id).ToList();
		AccountRepository repository = new(_context);

		// Act
		await repository.DeleteAsync(idsToDelete, CancellationToken.None);
		List<AccountEntity> remainingEntities = await _context.Accounts.ToListAsync();

		// Assert
		Assert.Single(remainingEntities);
		Assert.DoesNotContain(remainingEntities, e => idsToDelete.Contains(e.Id));
	}

	[Fact]
	public async Task ExistsAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		_context.ResetDatabase();

		AccountEntity entity = AccountEntityGenerator.Generate();
		await _context.Accounts.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_context);

		// Act
		bool result = await repository.ExistsAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public async Task ExistsAsync_NonExistingId_ReturnsFalse()
	{
		// Arrange
		_context.ResetDatabase();

		AccountRepository repository = new(_context);

		// Act
		bool result = await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public async Task GetCountAsync_ReturnsCorrectCount()
	{
		// Arrange
		_context.ResetDatabase();

		List<AccountEntity> entities = AccountEntityGenerator.GenerateList(3);
		await _context.Accounts.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		AccountRepository repository = new(_context);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, count);
	}
}