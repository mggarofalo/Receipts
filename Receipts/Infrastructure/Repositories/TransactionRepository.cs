using Application.Interfaces;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class TransactionRepository(ApplicationDbContext context, IMapper mapper) : ITransactionRepository
{
	private readonly ApplicationDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		TransactionEntity? entity = await _context.Transactions
			.FindAsync([id], cancellationToken);

		return _mapper.Map<Transaction>(entity);
	}

	public async Task<List<Transaction>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<TransactionEntity> entities = await _context.Transactions
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Transaction>).ToList();
	}

	public async Task<List<Transaction>> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		List<TransactionEntity> entities = await _context.Transactions
			.Where(x => x.ReceiptId == receiptId)
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Transaction>).ToList();
	}

	public async Task<List<Transaction>> CreateAsync(List<Transaction> transactions, CancellationToken cancellationToken)
	{
		List<TransactionEntity> createdEntities = [];

		foreach (TransactionEntity entity in transactions.Select(_mapper.Map<TransactionEntity>).ToList())
		{
			EntityEntry<TransactionEntity> entityEntry = await _context.Transactions.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		return createdEntities.Select(_mapper.Map<Transaction>).ToList();
	}

	public async Task<bool> UpdateAsync(List<Transaction> transactions, CancellationToken cancellationToken)
	{
		List<TransactionEntity> newEntities = transactions.Select(_mapper.Map<TransactionEntity>).ToList();

		foreach (TransactionEntity newEntity in newEntities)
		{
			TransactionEntity existingEntity = await _context.Transactions.SingleAsync(e => e.Id == newEntity.Id, cancellationToken);
			existingEntity.ReceiptId = newEntity.ReceiptId;
			existingEntity.AccountId = newEntity.AccountId;
			existingEntity.Amount = newEntity.Amount;
			existingEntity.Date = newEntity.Date;
		}

		return true;
	}

	public async Task<bool> DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<TransactionEntity> entities = await _context.Transactions.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
		_context.Transactions.RemoveRange(entities);

		return true;
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await _context.Transactions.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await _context.Transactions.CountAsync(cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await _context.SaveChangesAsync(cancellationToken);
	}
}