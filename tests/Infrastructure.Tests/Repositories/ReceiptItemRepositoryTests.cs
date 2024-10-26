using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class ReceiptItemRepositoryTests
{
	private readonly ApplicationDbContext _context = DbContextHelpers.CreateInMemoryContext();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsReceiptItem()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptItemEntity entity = ReceiptItemEntityGenerator.Generate();
		await _context.ReceiptItems.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_context);

		// Act
		ReceiptItemEntity? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(entity, actual);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptItemRepository repository = new(_context);

		// Act
		ReceiptItemEntity? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);
	}

	[Fact]
	public async Task GetByReceiptIdAsync_ExistingReceiptId_ReturnsReceiptItems()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await _context.Receipts.AddAsync(receipt);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(3, receipt.Id);
		await _context.ReceiptItems.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_context);

		// Act
		List<ReceiptItemEntity>? actual = await repository.GetByReceiptIdAsync(receipt.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(entities.Count, actual.Count);
		Assert.Equal(entities, actual);
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllReceiptItems()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await _context.Receipts.AddAsync(receipt);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(3, receipt.Id);
		await _context.ReceiptItems.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_context);

		// Act
		List<ReceiptItemEntity> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);
		Assert.Equal(entities, actual);
	}

	[Fact]
	public async Task CreateAsync_ValidReceiptItems_ReturnsCreatedReceiptItems()
	{
		// Arrange
		_context.ResetDatabase();

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(2);
		entities.ForEach(e => e.Id = Guid.Empty);
		ReceiptItemRepository repository = new(_context);

		// Act
		List<ReceiptItemEntity> actual = await repository.CreateAsync(entities, CancellationToken.None);

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
	public async Task UpdateAsync_ValidReceiptItems_UpdatesReceiptItems()
	{
		// Arrange
		_context.ResetDatabase();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await _context.Receipts.AddAsync(receipt);

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(2, receipt.Id);
		await _context.ReceiptItems.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_context);

		// Modify receipt items
		entities.ForEach(e =>
		{
			e.Description = "Updated " + e.Description;
			e.Quantity++;
		});

		// Act
		await repository.UpdateAsync(entities, CancellationToken.None);
		List<ReceiptItemEntity> updatedEntities = await _context.ReceiptItems.ToListAsync();

		// Assert
		Assert.Equal(entities.Count, updatedEntities.Count);

		foreach (ReceiptItemEntity expectedItem in entities)
		{
			ReceiptItemEntity? updatedEntity = updatedEntities.FirstOrDefault(e => e.Id == expectedItem.Id);
			Assert.NotNull(updatedEntity);
			Assert.Equal(expectedItem.Description, updatedEntity.Description);
			Assert.Equal(expectedItem.Quantity, updatedEntity.Quantity);
		}
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesReceiptItems()
	{
		// Arrange
		_context.ResetDatabase();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await _context.Receipts.AddAsync(receipt);

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(3, receipt.Id);
		await _context.ReceiptItems.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		List<Guid> idsToDelete = entities.Take(2).Select(e => e.Id).ToList();
		ReceiptItemRepository repository = new(_context);

		// Act
		await repository.DeleteAsync(idsToDelete, CancellationToken.None);
		List<ReceiptItemEntity> remainingEntities = await _context.ReceiptItems.ToListAsync();

		// Assert
		Assert.Single(remainingEntities);
		Assert.DoesNotContain(remainingEntities, e => idsToDelete.Contains(e.Id));
	}

	[Fact]
	public async Task ExistsAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptItemEntity entity = ReceiptItemEntityGenerator.Generate();
		await _context.ReceiptItems.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_context);

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

		ReceiptItemRepository repository = new(_context);

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

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(3);
		await _context.ReceiptItems.AddRangeAsync(entities);
		await _context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_context);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, count);
	}
}
