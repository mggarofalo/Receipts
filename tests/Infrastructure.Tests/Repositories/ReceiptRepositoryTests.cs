using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class ReceiptRepositoryTests
{
	private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsReceipt()
	{
		// Arrange
		const int expectedCount = 1;
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		ReceiptEntity entity = ReceiptEntityGenerator.Generate();
		await context.Receipts.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(expectedCount, await context.Receipts.CountAsync());

		ReceiptRepository repository = new(_contextFactory);

		// Act
		ReceiptEntity? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

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
		ReceiptRepository repository = new(_contextFactory);
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		Assert.Equal(expectedCount, await context.Receipts.CountAsync());

		// Act
		ReceiptEntity? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllReceipts()
	{
		// Arrange
		const int expectedReceiptCount = 2;
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(expectedReceiptCount);
		await context.Receipts.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(expectedReceiptCount, await context.Receipts.CountAsync());

		ReceiptRepository repository = new(_contextFactory);

		// Act
		List<ReceiptEntity> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expectedReceiptCount, actual.Count);
		Assert.Equal(entities, actual);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CreateAsync_ValidReceipts_ReturnsCreatedReceipts()
	{
		// Arrange
		const int expectedReceiptCount = 2;
		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(expectedReceiptCount);
		entities.ForEach(e => e.Id = Guid.Empty);
		ReceiptRepository repository = new(_contextFactory);

		// Act
		List<ReceiptEntity> actual = await repository.CreateAsync(entities, CancellationToken.None);

		// Assert
		Assert.Equal(expectedReceiptCount, actual.Count);

		Assert.All(actual, r =>
		{
			Assert.NotEqual(Guid.Empty, r.Id);
		});

		Assert.Equal(entities, actual.Select(r =>
		{
			r.Id = Guid.Empty;
			return r;
		}));

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		Assert.Equal(expectedReceiptCount, await verifyContext.Receipts.CountAsync());

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task UpdateAsync_ValidReceipt_UpdatesReceipt()
	{
		// Arrange
		const int expectedReceiptCount = 2;
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(expectedReceiptCount);
		await context.Receipts.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(expectedReceiptCount, await context.Receipts.CountAsync());

		ReceiptRepository repository = new(_contextFactory);

		// Modify receipt
		entities.ForEach(e =>
		{
			e.Description = "Updated " + e.Description;
			e.TaxAmount += 1.0m;
		});

		// Act
		await repository.UpdateAsync(entities, CancellationToken.None);

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		List<ReceiptEntity> updatedEntities = await verifyContext.Receipts.ToListAsync();

		// Assert
		Assert.Equal(expectedReceiptCount, updatedEntities.Count);

		foreach (ReceiptEntity expectedReceipt in entities)
		{
			ReceiptEntity? updatedEntity = updatedEntities.FirstOrDefault(e => e.Id == expectedReceipt.Id);
			Assert.NotNull(updatedEntity);
			Assert.Equal(expectedReceipt.Description, updatedEntity.Description);
			Assert.Equal(expectedReceipt.TaxAmount, updatedEntity.TaxAmount);
		}

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesReceipts()
	{
		// Arrange
		const int initialReceiptCount = 5;
		const int deleteCount = 2;
		const int expectedRemainingCount = 3;
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(initialReceiptCount);
		await context.Receipts.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(initialReceiptCount, await context.Receipts.CountAsync());

		List<Guid> idsToDelete = entities.Take(deleteCount).Select(e => e.Id).ToList();
		ReceiptRepository repository = new(_contextFactory);

		// Act
		await repository.DeleteAsync(idsToDelete, CancellationToken.None);

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		List<ReceiptEntity> remainingEntities = await verifyContext.Receipts.ToListAsync();

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
		ReceiptEntity entity = ReceiptEntityGenerator.Generate();
		await context.Receipts.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(expectedCount, await context.Receipts.CountAsync());

		ReceiptRepository repository = new(_contextFactory);

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
		ReceiptRepository repository = new(_contextFactory);
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		Assert.Equal(expectedCount, await context.Receipts.CountAsync());

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
		const int expectedReceiptCount = 3;
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<ReceiptEntity> entities = ReceiptEntityGenerator.GenerateList(expectedReceiptCount);
		await context.Receipts.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);
		Assert.Equal(expectedReceiptCount, await context.Receipts.CountAsync());

		ReceiptRepository repository = new(_contextFactory);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(expectedReceiptCount, count);

		_contextFactory.ResetDatabase();
	}
}
