using FluentAssertions;
using Infrastructure.Entities.Core;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using SampleData.Entities;

namespace Infrastructure.Tests.Repositories;

public class ReceiptItemRepositoryTests
{
	private readonly IDbContextFactory<ApplicationDbContext> _contextFactory = DbContextHelpers.CreateInMemoryContextFactory();

	private async Task<ReceiptEntity> CreateParentReceiptAsync()
	{
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		ReceiptEntity receipt = ReceiptEntityGenerator.Generate();
		await context.Receipts.AddAsync(receipt);
		await context.SaveChangesAsync(CancellationToken.None);
		return receipt;
	}

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsReceiptItem()
	{
		// Arrange
		ReceiptEntity receipt = await CreateParentReceiptAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		ReceiptItemEntity entity = ReceiptItemEntityGenerator.Generate(receipt.Id);
		await context.ReceiptItems.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_contextFactory);

		// Act
		ReceiptItemEntity? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeEquivalentTo(entity, opt => opt.Excluding(member => member.Name == nameof(ReceiptItemEntity.Receipt)));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		ReceiptItemRepository repository = new(_contextFactory);

		// Act
		ReceiptItemEntity? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.Null(result);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetByReceiptIdAsync_ExistingReceiptId_ReturnsReceiptItems()
	{
		// Arrange
		const int expectedItemCount = 3;
		ReceiptEntity receipt = await CreateParentReceiptAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(expectedItemCount, receipt.Id);
		await context.ReceiptItems.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_contextFactory);

		// Act
		List<ReceiptItemEntity>? actual = await repository.GetByReceiptIdAsync(receipt.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		actual.Should().BeEquivalentTo(entities, opt => opt.Excluding(member => member.Name == nameof(ReceiptItemEntity.Receipt)));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetAllAsync_ReturnsAllReceiptItems()
	{
		// Arrange
		const int expectedItemCount = 3;
		ReceiptEntity receipt = await CreateParentReceiptAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(expectedItemCount, receipt.Id);
		await context.ReceiptItems.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_contextFactory);

		// Act
		List<ReceiptItemEntity> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		actual.Should().BeEquivalentTo(entities, opt => opt.Excluding(member => member.Name == nameof(ReceiptItemEntity.Receipt)));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task CreateAsync_ValidReceiptItems_ReturnsCreatedReceiptItems()
	{
		// Arrange
		const int expectedItemCount = 2;
		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(expectedItemCount);
		entities.ForEach(e => e.Id = Guid.Empty);
		ReceiptItemRepository repository = new(_contextFactory);

		// Act
		List<ReceiptItemEntity> actual = await repository.CreateAsync(entities, CancellationToken.None);

		// Assert
		Assert.All(actual, r =>
		{
			Assert.NotEqual(Guid.Empty, r.Id);
		});

		actual.Should().BeEquivalentTo(entities, opt => opt.Excluding(x => x.Id));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task UpdateAsync_ValidReceiptItems_UpdatesReceiptItems()
	{
		// Arrange
		const int expectedItemCount = 2;
		ReceiptEntity receipt = await CreateParentReceiptAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(expectedItemCount, receipt.Id);
		await context.ReceiptItems.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_contextFactory);

		// Modify receipt items
		entities.ForEach(e =>
		{
			e.Description = "Updated " + e.Description;
			e.Quantity++;
		});

		// Act
		await repository.UpdateAsync(entities, CancellationToken.None);

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		List<ReceiptItemEntity> updatedEntities = await verifyContext.ReceiptItems.ToListAsync();

		// Assert
		updatedEntities.Should().BeEquivalentTo(entities, opt => opt.Excluding(member => member.Name == nameof(ReceiptItemEntity.Receipt)));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task DeleteAsync_ValidIds_DeletesReceiptItems()
	{
		// Arrange
		const int initialItemCount = 5;
		const int itemsToDeleteCount = 2;
		const int expectedRemainingCount = 3;

		ReceiptEntity receipt = await CreateParentReceiptAsync();
		using ApplicationDbContext context = _contextFactory.CreateDbContext();

		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(initialItemCount, receipt.Id);
		await context.ReceiptItems.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		List<Guid> idsToDelete = [.. entities.Take(itemsToDeleteCount).Select(e => e.Id)];
		ReceiptItemRepository repository = new(_contextFactory);

		// Act
		await repository.DeleteAsync(idsToDelete, CancellationToken.None);

		using ApplicationDbContext verifyContext = _contextFactory.CreateDbContext();
		List<ReceiptItemEntity> remainingEntities = await verifyContext.ReceiptItems.ToListAsync();

		// Assert
		remainingEntities.Count.Should().Be(expectedRemainingCount);
		Assert.DoesNotContain(remainingEntities, e => idsToDelete.Contains(e.Id));

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task ExistsAsync_ExistingId_ReturnsTrue()
	{
		// Arrange
		const int expectedCount = 1;
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		ReceiptItemEntity entity = ReceiptItemEntityGenerator.Generate();
		await context.ReceiptItems.AddAsync(entity);
		await context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_contextFactory);

		// Act
		bool result = await repository.ExistsAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.True(result);
		(await context.ReceiptItems.CountAsync()).Should().Be(expectedCount);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task ExistsAsync_NonExistingId_ReturnsFalse()
	{
		// Arrange
		const int expectedCount = 0;
		ReceiptItemRepository repository = new(_contextFactory);

		// Act
		bool result = await repository.ExistsAsync(Guid.NewGuid(), CancellationToken.None);

		// Assert
		Assert.False(result);
		(await _contextFactory.CreateDbContext().ReceiptItems.CountAsync()).Should().Be(expectedCount);

		_contextFactory.ResetDatabase();
	}

	[Fact]
	public async Task GetCountAsync_ReturnsCorrectCount()
	{
		// Arrange
		const int expectedCount = 3;
		using ApplicationDbContext context = _contextFactory.CreateDbContext();
		List<ReceiptItemEntity> entities = ReceiptItemEntityGenerator.GenerateList(expectedCount);
		await context.ReceiptItems.AddRangeAsync(entities);
		await context.SaveChangesAsync(CancellationToken.None);

		ReceiptItemRepository repository = new(_contextFactory);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		count.Should().Be(expectedCount);

		_contextFactory.ResetDatabase();
	}
}
