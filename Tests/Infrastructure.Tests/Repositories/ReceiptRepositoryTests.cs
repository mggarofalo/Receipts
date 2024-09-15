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

public class ReceiptRepositoryTests
{
	private readonly IMapper _mapper = RepositoryHelpers.CreateMapper<ReceiptMappingProfile>();
	private readonly ApplicationDbContext _context = DbContextHelpers.CreateInMemoryContext();

	[Fact]
	public async Task GetByIdAsync_ExistingId_ReturnsReceipt()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptEntity entity = ReceiptEntityGenerator.Generate();
		await _context.Receipts.AddAsync(entity);
		await _context.SaveChangesAsync(CancellationToken.None);

		Receipt expected = _mapper.Map<Receipt>(entity);
		ReceiptRepository repository = new(_context, _mapper);

		// Act
		Receipt? actual = await repository.GetByIdAsync(entity.Id, CancellationToken.None);

		// Assert
		Assert.NotNull(actual);
		Assert.Equal(expected, actual);
	}

	[Fact]
	public async Task GetByIdAsync_NonExistingId_ReturnsNull()
	{
		// Arrange
		_context.ResetDatabase();

		ReceiptRepository repository = new(_context, _mapper);

		// Act
		Receipt? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

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

		List<Receipt> expected = entities.Select(_mapper.Map<Receipt>).ToList();
		ReceiptRepository repository = new(_context, _mapper);

		// Act
		List<Receipt> actual = await repository.GetAllAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, actual.Count);
		Assert.All(actual, r => Assert.Contains(r, expected));
		Assert.All(expected, r => Assert.Contains(r, actual));
	}

	[Fact]
	public async Task CreateAsync_ValidReceipts_ReturnsCreatedReceipts()
	{
		// Arrange
		_context.ResetDatabase();

		List<Receipt> expected = ReceiptGenerator.GenerateList(2);
		expected.ForEach(e => e.Id = null);
		ReceiptRepository repository = new(_context, _mapper);

		// Act
		List<Receipt> actual = await repository.CreateAsync(expected, CancellationToken.None);

		// Assert
		Assert.Equal(expected.Count, actual.Count);

		Assert.All(actual, r =>
		{
			// Ensure IDs were correctly set by the database
			Assert.NotEqual(Guid.Empty, r.Id);
			Assert.NotNull(r.Id);
		});

		Assert.Equal(expected, actual.Select(r =>
		{
			// Compare expected (null IDs) to actual (overwrite IDs with null)
			r.Id = null;
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

		List<Receipt> expected = entities.Select(_mapper.Map<Receipt>).ToList();
		ReceiptRepository repository = new(_context, _mapper);

		// Modify receipt
		expected.ForEach(e =>
		{
			e.Description = "Updated " + e.Description;
			e.TaxAmount = new Money(e.TaxAmount.Amount + 1.0m, e.TaxAmount.Currency);
		});

		// Act
		await repository.UpdateAsync(expected, CancellationToken.None);
		List<ReceiptEntity> updatedEntities = await _context.Receipts.ToListAsync();

		// Assert
		Assert.Equal(expected.Count, updatedEntities.Count);

		foreach (Receipt expectedReceipt in expected)
		{
			ReceiptEntity? updatedEntity = updatedEntities.FirstOrDefault(e => e.Id == expectedReceipt.Id);
			Assert.NotNull(updatedEntity);
			Assert.Equal(expectedReceipt.Description, updatedEntity.Description);
			Assert.Equal(expectedReceipt.TaxAmount.Amount, updatedEntity.TaxAmount);
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
		ReceiptRepository repository = new(_context, _mapper);

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

		ReceiptRepository repository = new(_context, _mapper);

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

		ReceiptRepository repository = new(_context, _mapper);

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

		ReceiptRepository repository = new(_context, _mapper);

		// Act
		int count = await repository.GetCountAsync(CancellationToken.None);

		// Assert
		Assert.Equal(entities.Count, count);
	}
}
