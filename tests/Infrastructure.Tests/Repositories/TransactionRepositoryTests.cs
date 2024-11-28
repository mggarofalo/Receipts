using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class TransactionRepositoryTests
{
	private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

	private async Task<(ReceiptEntity receipt, AccountEntity account)> CreateParentEntitiesAsync()
	{
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await context.Receipts.AddAsync(receipt);

		AccountEntity account = AccountEntityGenerator.Generate();
		await context.Accounts.AddAsync(account);

		await context.SaveChangesAsync(CancellationToken.None);
		return (receipt, account);
	}

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsTransaction()
	{
		// Arrange
		(ReceiptEntity receipt, AccountEntity account) = await CreateParentEntitiesAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		TransactionEntity entity = TransactionEntityGenerator.Generate(receipt.Id, account.Id);
		await context.Transactions.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);

		TransactionRepository repository = new(_contextFactory);

		// Act
		TransactionEntity? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(entity, actual);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		const int expectedCount = 0;
		TransactionRepository repository = new(_contextFactory);
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		Assert.Equal(expectedCount, await context.Transactions.CountAsync());

		// Act
		TransactionEntity? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByReceiptIdAsync_ExistingReceiptId_ReturnsTransactions()
	{
		// Arrange
		const int expectedTransactionCount = 3;
		(ReceiptEntity receipt, AccountEntity account) = await CreateParentEntitiesAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(expectedTransactionCount, receipt.Id, account.Id);
		await context.Transactions.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		TransactionRepository repository = new(_contextFactory);

		// Act
		List<TransactionEntity>? actual = await repository.GetByReceiptIdAsync(receipt.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expectedTransactionCount, actual.Count);
		Assert.Equal(entities, actual);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllTransactions()
	{
		// Arrange
		const int expectedTransactionCount = 3;
		(ReceiptEntity receipt, AccountEntity account) = await CreateParentEntitiesAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(expectedTransactionCount, receipt.Id, account.Id);
		await context.Transactions.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(expectedTransactionCount, await context.Transactions.CountAsync());

		TransactionRepository repository = new(_contextFactory);

		// Act
		List<TransactionEntity> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expectedTransactionCount, actual.Count);
		Assert.Equal(entities, actual);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CreateAsync_ValidTransactions_ReturnsCreatedTransactions()
	{
		// Arrange
		const int expectedTransactionCount = 2;
		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(expectedTransactionCount);
		entities.ForEach(e => e.Id = Guid.Empty);
		TransactionRepository repository = new(_contextFactory);

		// Act
		List<TransactionEntity> actual = await repository.CreateAsync(entities, CancellationToken.None);

		// Assert
		Assert.Equal(expectedTransactionCount, actual.Count);

		Assert.All(actual, t =>
		{
			Assert.NotEqual(Guid.Empty, t.Id);
		});

		Assert.Equal(entities, actual.Select(t =>
		{
			t.Id = Guid.Empty;
			return t;
		}));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task UpdateAsync_ValidTransactions_UpdatesTransactions()
	{
		// Arrange
		const int expectedTransactionCount = 2;
		(ReceiptEntity receipt, AccountEntity account) = await CreateParentEntitiesAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(expectedTransactionCount, receipt.Id, account.Id);
		await context.Transactions.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(expectedTransactionCount, await context.Transactions.CountAsync());

		TransactionRepository repository = new(_contextFactory);

		// Modify transactions
		entities.ForEach(e =>
		{
			e.Amount += 10.0m;
			e.Date = e.Date.AddDays(1);
		});

		// Act
		await repository.UpdateAsync(entities, CancellationToken.None);

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		List<TransactionEntity> updatedEntities = await verifyContext.Transactions.ToListAsync();

		// Assert
		Assert.Equal(expectedTransactionCount, updatedEntities.Count);

		foreach (TransactionEntity expectedTransaction in entities)
		{
			TransactionEntity? updatedEntity = updatedEntities.FirstOrDefault(e => e.Id == expectedTransaction.Id);
			Assert.NotNull(updatedEntity);
			Assert.Equal(expectedTransaction.Amount, updatedEntity.Amount);
			Assert.Equal(expectedTransaction.Date, updatedEntity.Date);
		}

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesTransactions()
	{
		// Arrange
		const int initialTransactionCount = 5;
		const int transactionsToDeleteCount = 2;
		const int expectedRemainingCount = 3;

		(ReceiptEntity receipt, AccountEntity account) = await CreateParentEntitiesAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(initialTransactionCount, receipt.Id, account.Id);
		await context.Transactions.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(initialTransactionCount, await context.Transactions.CountAsync());

		List<Guid> idsToDelete = entities.Take(transactionsToDeleteCount).Select(e => e.Id).ToList();
		TransactionRepository repository = new(_contextFactory);

		// Act
		await repository.DeleteAsync(idsToDelete, CancellationToken.None);

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		List<TransactionEntity> remainingEntities = await verifyContext.Transactions.ToListAsync();

		// Assert
		Assert.Equal(expectedRemainingCount, remainingEntities.Count);
		Assert.DoesNotContain(remainingEntities, e => idsToDelete.Contains(e.Id));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task ExistsAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		const int expectedCount = 1;
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		TransactionEntity entity = TransactionEntityGenerator.Generate();
		await context.Transactions.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(expectedCount, await context.Transactions.CountAsync());

		TransactionRepository repository = new(_contextFactory);

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
		const int expectedCount = 0;
		TransactionRepository repository = new(_contextFactory);
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		Assert.Equal(expectedCount, await context.Transactions.CountAsync());

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
		const int expectedTransactionCount = 3;
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(expectedTransactionCount);
		await context.Transactions.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(expectedTransactionCount, await context.Transactions.CountAsync());

		TransactionRepository repository = new(_contextFactory);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expectedTransactionCount, count);

		_contextFactory.ResetDatabase();
	}
}
