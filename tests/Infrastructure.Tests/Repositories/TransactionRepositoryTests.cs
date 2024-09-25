using AutoMapper;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Domain.Core;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class TransactionRepositoryTests
{
	private readonly IMapper _mapper = RepositoryHelpers.CreateMapper<TransactionMappingProfile>();
	private readonly ApplicationDbContext _context = DbContextHelpers.CreateInMemoryContext();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsTransaction()
	{
		// Arrange
		_context.ResetDatabase();

		TransactionEntity entity = TransactionEntityGenerator.Generate();
		await _context.Transactions.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		Transaction expected = _mapper.Map<Transaction>(entity);
		TransactionRepository repository = new(_context, _mapper);

		// Act
		Transaction? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		_context.ResetDatabase();

		TransactionRepository repository = new(_context, _mapper);

		// Act
		Transaction? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

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

		List<Transaction> expected = entities.Select(_mapper.Map<Transaction>).ToList();
		TransactionRepository repository = new(_context, _mapper);

		// Act
		List<Transaction>? actual = await repository.GetByReceiptIdAsync(receipt.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected.Count, actual.Count);
		Assert.Equal(expected, actual);
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

		List<Transaction> expected = entities.Select(_mapper.Map<Transaction>).ToList();
		TransactionRepository repository = new(_context, _mapper);

		// Act
		List<Transaction> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task CreateAsync_ValidTransactions_ReturnsCreatedTransactions()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		AccountEntity account = AccountEntityGenerator.Generate();
		await _context.Receipts.AddAsync(receipt);
		await _context.Accounts.AddAsync(account);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<Transaction> expected = TransactionGenerator.GenerateList(2);
		expected.ForEach(e => e.Id = null);
		TransactionRepository repository = new(_context, _mapper);

		// Act
		List<Transaction> actual = await repository.CreateAsync(expected, receipt.Id, account.Id, CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);

		Assert.All(actual, t =>
		{
			Assert.NotEqual(Guid.Empty, t.Id);
			Assert.NotNull(t.Id);
		});

		Assert.Equal(expected, actual.Select(t =>
		{
			t.Id = null;
			return t;
		}));
	}

	[Fact]
	public async Task UpdateAsync_ValidTransactions_UpdatesTransactions()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		AccountEntity account = AccountEntityGenerator.Generate();
		await _context.Receipts.AddAsync(receipt);
		await _context.Accounts.AddAsync(account);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(2, receipt.Id, account.Id);
		await _context.Transactions.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<Transaction> expected = entities.Select(_mapper.Map<Transaction>).ToList();
		TransactionRepository repository = new(_context, _mapper);

		// Modify transactions
		expected.ForEach(e =>
		{
			e.Amount = new Money(e.Amount.Amount + 10.0m, e.Amount.Currency);
			e.Date = e.Date.AddDays(1);
		});

		// Act
		await repository.UpdateAsync(expected, receipt.Id, account.Id, CancellationToken.None);
		List<TransactionEntity> updatedEntities = await _context.Transactions.ToListAsync();

		// Assert
		Assert.Equal(expected.Count, updatedEntities.Count);

		foreach (Transaction expectedTransaction in expected)
		{
			TransactionEntity? updatedEntity = updatedEntities.FirstOrDefault(e => e.Id == expectedTransaction.Id);
			Assert.NotNull(updatedEntity);
			Assert.Equal(expectedTransaction.Amount.Amount, updatedEntity.Amount);
			Assert.Equal(expectedTransaction.Date, updatedEntity.Date);
		}
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesTransactions()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		AccountEntity account = AccountEntityGenerator.Generate();
		await _context.Receipts.AddAsync(receipt);
		await _context.Accounts.AddAsync(account);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<TransactionEntity> entities = TransactionEntityGenerator.GenerateList(3, receipt.Id, account.Id);
		await _context.Transactions.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<Guid> idsToDelete = entities.Take(2).Select(e => e.Id).ToList();
		TransactionRepository repository = new(_context, _mapper);

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

		TransactionRepository repository = new(_context, _mapper);

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

		TransactionRepository repository = new(_context, _mapper);

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

		TransactionRepository repository = new(_context, _mapper);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, count);
	}
}
