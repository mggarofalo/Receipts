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

	public async Task<List<Transaction>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<TransactionEntity> entities = await context.Transactions
			.ToListAsync(cancellationToken);

		return entities.Select(mapper.Map<Transaction>).ToList();
	}

	public async Task<List<Transaction>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		bool receiptExists = await ExistsAsync(receiptId, cancellationToken);

		if (!receiptExists)
		{
			return null;
		}

		List<TransactionEntity> entities = await context.Transactions
			.Where(x => x.ReceiptId == receiptId)
			.ToListAsync(cancellationToken);

		return entities.Select(mapper.Map<Transaction>).ToList();
	}

	public async Task<List<Transaction>> CreateAsync(List<Transaction> models, Guid accountId, Guid receiptId, CancellationToken cancellationToken)
	{
		bool receiptExists = await ExistsAsync(receiptId, cancellationToken);

		if (!receiptExists)
		{
			throw new ArgumentException("Receipt does not exist.", nameof(receiptId));
		}

		bool accountExists = await ExistsAsync(accountId, cancellationToken);

		if (!accountExists)
		{
			throw new ArgumentException("Account does not exist.", nameof(accountId));
		}

		List<TransactionEntity> createdEntities = [];

		foreach (TransactionEntity entity in models.Select(domain => mapper.MapToTransactionEntity(domain, accountId, receiptId)))
		{
			EntityEntry<TransactionEntity> entityEntry = await context.Transactions.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		return createdEntities.Select(mapper.Map<Transaction>).ToList();
	}

	public async Task UpdateAsync(List<Transaction> models, Guid accountId, Guid receiptId, CancellationToken cancellationToken)
	{
		bool receiptExists = await ExistsAsync(receiptId, cancellationToken);

		if (!receiptExists)
		{
			throw new ArgumentException("Receipt does not exist.", nameof(receiptId));
		}

		bool accountExists = await ExistsAsync(accountId, cancellationToken);

		if (!accountExists)
		{
			throw new ArgumentException("Account does not exist.", nameof(accountId));
		}

		List<TransactionEntity> newEntities = models.Select(domain => mapper.MapToTransactionEntity(domain, accountId, receiptId)).ToList();

		foreach (TransactionEntity newEntity in newEntities)
		{
			TransactionEntity existingEntity = await context.Transactions.SingleAsync(e => e.Id == newEntity.Id, cancellationToken);
			existingEntity.ReceiptId = newEntity.ReceiptId;
			existingEntity.AccountId = newEntity.AccountId;
			existingEntity.Amount = newEntity.Amount;
			existingEntity.Date = newEntity.Date;
		}
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<TransactionEntity> entities = await context.Transactions.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
		context.Transactions.RemoveRange(entities);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await context.Transactions.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await context.Transactions.CountAsync(cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await context.SaveChangesAsync(cancellationToken);
	}
}