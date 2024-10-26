using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class TransactionRepositoryTests
{
	private readonly ApplicationDbContext _context = DbContextHelpers.CreateInMemoryContext();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsTransaction()
	{
		// Arrange
		_context.ResetDatabase();

		TransactionEntity entity = TransactionEntityGenerator.Generate();
		await _context.Transactions.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		TransactionRepository repository = new(_context);

		// Act
		TransactionEntity? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(entity, actual);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		_context.ResetDatabase();

		TransactionRepository repository = new(_context);

		// Act
		TransactionEntity? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetByReceiptIdAsync_ExistingReceiptId_ReturnsTransactions()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		AccountEntity account = AccountEntityGenerator.Generate();
		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(3, receipt.Id, account.Id);
		await _context.Receipts.AddAsync(receipt);
		await _context.Accounts.AddAsync(account);
		await _context.Transactions.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		TransactionRepository repository = new(_context);

		// Act
		List<TransactionEntity>? actual = await repository.GetByReceiptIdAsync(receipt.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(entities.Count, actual.Count);
		Assert.Equal(entities, actual);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllTransactions()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		AccountEntity account = AccountEntityGenerator.Generate();
		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(3, receipt.Id, account.Id);
		await _context.Receipts.AddAsync(receipt);
		await _context.Accounts.AddAsync(account);
		await _context.Transactions.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		TransactionRepository repository = new(_context);

		// Act
		List<TransactionEntity> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);
		Assert.Equal(entities, actual);
	}

	[Fact]
	public async Task CreateAsync_ValidTransactions_ReturnsCreatedTransactions()
	{
		// Arrange
		_context.ResetDatabase();

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(2);
		entities.ForEach(e => e.Id = Guid.Empty);
		TransactionRepository repository = new(_context);

		// Act
		List<TransactionEntity> actual = await repository.CreateAsync(entities, CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);

		Assert.All(actual, t =>
		{
			Assert.NotEqual(Guid.Empty, t.Id);
		});

		Assert.Equal(entities, actual.Select(t =>
		{
			t.Id = Guid.Empty;
			return t;
		}));
	}

	[Fact]
	public async Task UpdateAsync_ValidTransactions_UpdatesTransactions()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await _context.Receipts.AddAsync(receipt);

		AccountEntity account = AccountEntityGenerator.Generate();
		await _context.Accounts.AddAsync(account);

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(2, receipt.Id, account.Id);
		await _context.Transactions.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		TransactionRepository repository = new(_context);

		// Modify transactions
		entities.ForEach(e =>
		{
			e.Amount += 10.0m;
			e.Date = e.Date.AddDays(1);
		});

		// Act
		await repository.UpdateAsync(entities, CancellationToken.None);
		List<TransactionEntity> updatedEntities = await _context.Transactions.ToListAsync();

		// Assert
		Assert.Equal(entities.Count, updatedEntities.Count);

		foreach (TransactionEntity expectedTransaction in entities)
		{
			TransactionEntity? updatedEntity = updatedEntities.FirstOrDefault(e => e.Id == expectedTransaction.Id);
			Assert.NotNull(updatedEntity);
			Assert.Equal(expectedTransaction.Amount, updatedEntity.Amount);
			Assert.Equal(expectedTransaction.Date, updatedEntity.Date);
		}
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesTransactions()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await _context.Receipts.AddAsync(receipt);

		AccountEntity account = AccountEntityGenerator.Generate();
		await _context.Accounts.AddAsync(account);

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(3, receipt.Id, account.Id);
		await _context.Transactions.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<Guid> idsToDelete = entities.Take(2).Select(e => e.Id).ToList();
		TransactionRepository repository = new(_context);

		// Act
		await repository.DeleteAsync(idsToDelete, CancellationToken.None);
		List<TransactionEntity> remainingEntities = await _context.Transactions.ToListAsync();

		// Assert
		Assert.Single(remainingEntities);
		Assert.DoesNotContain(remainingEntities, e => idsToDelete.Contains(e.Id));
	}

	[Fact]
	public async Task ExistsAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		_context.ResetDatabase();

		TransactionEntity entity = TransactionEntityGenerator.Generate();
		await _context.Transactions.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		TransactionRepository repository = new(_context);

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

		TransactionRepository repository = new(_context);

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

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(3);
		await _context.Transactions.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		TransactionRepository repository = new(_context);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, count);
	}
}
