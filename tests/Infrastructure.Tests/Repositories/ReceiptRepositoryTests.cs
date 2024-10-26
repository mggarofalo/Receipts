using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class ReceiptRepositoryTests
{
	private readonly ApplicationDbContext _context = DbContextHelpers.CreateInMemoryContext();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsReceipt()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity entity = ReceiptEntityGenerator.Generate();
		await _context.Receipts.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptRepository repository = new(_context);

		// Act
		ReceiptEntity? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(entity, actual);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptRepository repository = new(_context);

		// Act
		ReceiptEntity? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllReceipts()
	{
		// Arrange
		_context.ResetDatabase();

		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(2);
		await _context.Receipts.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptRepository repository = new(_context);

		// Act
		List<ReceiptEntity> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);
		Assert.Equal(entities, actual);
	}

	[Fact]
	public async Task CreateAsync_ValidReceipts_ReturnsCreatedReceipts()
	{
		// Arrange
		_context.ResetDatabase();

		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(2);
		entities.ForEach(e => e.Id = Guid.Empty);
		ReceiptRepository repository = new(_context);

		// Act
		List<ReceiptEntity> actual = await repository.CreateAsync(entities, CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);

		Assert.All(actual, r =>
		{
			Assert.NotEqual(Guid.Empty, r.Id);
		});

		Assert.Equal(entities, actual.Select(r =>
		{
			r.Id = Guid.Empty;
			return r;
		}));
	}

	[Fact]
	public async Task UpdateAsync_ValidReceipt_UpdatesReceipt()
	{
		// Arrange
		_context.ResetDatabase();

		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(2);
		await _context.Receipts.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptRepository repository = new(_context);

		// Modify receipt
		entities.ForEach(e =>
		{
			e.Description = "Updated " + e.Description;
			e.TaxAmount += 1.0m;
		});

		// Act
		await repository.UpdateAsync(entities, CancellationToken.None);
		List<ReceiptEntity> updatedEntities = await _context.Receipts.ToListAsync();

		// Assert
		Assert.Equal(entities.Count, updatedEntities.Count);

		foreach (ReceiptEntity expectedReceipt in entities)
		{
			ReceiptEntity? updatedEntity = updatedEntities.FirstOrDefault(e => e.Id == expectedReceipt.Id);
			Assert.NotNull(updatedEntity);
			Assert.Equal(expectedReceipt.Description, updatedEntity.Description);
			Assert.Equal(expectedReceipt.TaxAmount, updatedEntity.TaxAmount);
		}
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesReceipts()
	{
		// Arrange
		_context.ResetDatabase();

		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(3);
		await _context.Receipts.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<Guid> idsToDelete = entities.Take(2).Select(e => e.Id).ToList();
		ReceiptRepository repository = new(_context);

		// Act
		await repository.DeleteAsync(idsToDelete, CancellationToken.None);
		List<ReceiptEntity> remainingEntities = await _context.Receipts.ToListAsync();

		// Assert
		Assert.Single(remainingEntities);
		Assert.DoesNotContain(remainingEntities, e => idsToDelete.Contains(e.Id));
	}

	[Fact]
	public async Task ExistsAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity entity = ReceiptEntityGenerator.Generate();
		await _context.Receipts.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptRepository repository = new(_context);

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

		ReceiptRepository repository = new(_context);

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

		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(3);
		await _context.Receipts.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptRepository repository = new(_context);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, count);
	}
}
