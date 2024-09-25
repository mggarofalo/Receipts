using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class TransactionRepository(ApplicationDbContext context, IMapper mapper) : ITransactionRepository
{
	public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		TransactionEntity? entity = await context.Transactions
			.FindAsync([id], cancellationToken);

		return mapper.Map<Transaction>(entity);
	}

	public async Task<List<Transaction>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		List<TransactionEntity> transactionEntities = await context.Transactions
			.Where(t => t.ReceiptId == receiptId)
			.AsNoTracking()
			.ToListAsync(cancellationToken);

		return transactionEntities.Select(mapper.Map<Transaction>).ToList();
	}

	public async Task<List<Transaction>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<TransactionEntity> transactionEntities = await context.Transactions
			.AsNoTracking()
			.ToListAsync(cancellationToken);

		return transactionEntities.Select(mapper.Map<Transaction>).ToList();
	}

	public async Task<List<Transaction>> CreateAsync(List<Transaction> models, Guid receiptId, Guid accountId, CancellationToken cancellationToken)
	{
		List<TransactionEntity> createdEntities = [];

		foreach (TransactionEntity entity in models.Select(m => mapper.MapToTransactionEntity(m, receiptId, accountId)))
		{
			EntityEntry<TransactionEntity> entityEntry = await context.Transactions.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		await context.SaveChangesAsync(cancellationToken);

		return createdEntities.Select(mapper.Map<Transaction>).ToList();
	}

	public async Task UpdateAsync(List<Transaction> models, Guid receiptId, Guid accountId, CancellationToken cancellationToken)
	{
		List<TransactionEntity> newEntities = models.Select(m => mapper.MapToTransactionEntity(m, receiptId, accountId)).ToList();

		foreach (TransactionEntity newEntity in newEntities)
		{
			TransactionEntity existingEntity = await context.Transactions.SingleAsync(e => e.Id == newEntity.Id, cancellationToken);
			existingEntity.ReceiptId = newEntity.ReceiptId;
			existingEntity.AccountId = newEntity.AccountId;
			existingEntity.Amount = newEntity.Amount;
			existingEntity.Date = newEntity.Date;
		}

		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<TransactionEntity> entities = await context.Transactions
			.Where(e => ids.Contains(e.Id))
			.ToListAsync(cancellationToken);

		context.Transactions.RemoveRange(entities);
		await context.SaveChangesAsync(cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Transactions.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await context.Transactions.CountAsync(cancellationToken);
	}
}