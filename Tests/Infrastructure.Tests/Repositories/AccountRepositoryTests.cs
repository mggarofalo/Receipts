using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class AccountRepositoryTests
{
	private readonly IMapper _mapper = RepositoryHelpers.CreateMapper<AccountMappingProfile>();
	private readonly ApplicationDbContext _context = RepositoryHelpers.CreateInMemoryContext();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsAccount()
	{
		// Arrange
		_context.ResetDatabase();

		AccountEntity entity = AccountEntityGenerator.Generate();
		await _context.Accounts.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		Account expected = _mapper.Map<Account>(entity);
		AccountRepository repository = new(_context, _mapper);

		// Act
		Account? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		_context.ResetDatabase();

		AccountRepository repository = new(_context, _mapper);

		// Act
		Account? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

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

		Account expected = _mapper.Map<Account>(accountEntity);
		AccountRepository repository = new(_context, _mapper);

		// Act
		Account? actual = await repository.GetByTransactionIdAsync(transactionEntity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task GetByTransactionIdAsync_NonExistingTransactionId_ReturnsNull()
	{
		// Arrange
		_context.ResetDatabase();

		AccountRepository repository = new(_context, _mapper);

		// Act
		Account? result = await repository.GetByTransactionIdAsync(Guid.NewGuid(), CancellationToken.None);

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

		List<Account> expected = entities.Select(_mapper.Map<Account>).ToList();
		AccountRepository repository = new(_context, _mapper);

		// Act
		List<Account> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);
		Assert.All(actual, r => Assert.Contains(r, expected));
		Assert.All(expected, a => Assert.Contains(a, actual));
	}

	[Fact]
	public async Task CreateAsync_ValidAccounts_ReturnsCreatedAccounts()
	{
		// Arrange
		_context.ResetDatabase();

		List<Account> expected = AccountGenerator.GenerateList(2);
		expected.ForEach(e => e.Id = null);
		AccountRepository repository = new(_context, _mapper);

		// Act
		List<Account> actual = await repository.CreateAsync(expected, CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);

		Assert.All(actual, a =>
		{
			// Ensure IDs were correctly set by the database
			Assert.NotEqual(Guid.Empty, a.Id);
			Assert.NotNull(a.Id);
		});

		Assert.Equal(expected, actual.Select(a =>
		{
			// Compare expected (null IDs) to actual (overwrite IDs with null)
			a.Id = null;
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

		List<Account> expected = entities.Select(_mapper.Map<Account>).ToList();
		AccountRepository repository = new(_context, _mapper);

		// Modify account
		expected.ForEach(e =>
		{
			e.Name = "Updated " + e.Name;
			e.IsActive = !e.IsActive;
		});

		// Act
		await repository.UpdateAsync(expected, CancellationToken.None);
		List<AccountEntity> updatedEntities = await _context.Accounts.ToListAsync();

		// Assert
		Assert.Equal(expected.Count, updatedEntities.Count);

		foreach (Account expectedAccount in expected)
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
		AccountRepository repository = new(_context, _mapper);

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

		AccountRepository repository = new(_context, _mapper);

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

		AccountRepository repository = new(_context, _mapper);

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

		AccountRepository repository = new(_context, _mapper);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, count);
	}
}