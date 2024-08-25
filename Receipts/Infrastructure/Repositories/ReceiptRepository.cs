using Application.Interfaces;
using AutoMapper;
using Common;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class ReceiptRepository(ApplicationDbContext context, IMapper mapper) : IReceiptRepository
{
	private readonly ApplicationDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		ReceiptEntity? entity = await _context.Receipts
			.FindAsync([id], cancellationToken);

		return _mapper.Map<Receipt>(entity);
	}

	public async Task<List<Receipt>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<ReceiptEntity> entities = await _context.Receipts
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Receipt>).ToList();
	}

	public async Task<List<Receipt>> GetByLocationAsync(string location, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> entities = await _context.Receipts
			.Where(e => e.Location == location)
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Receipt>).ToList();
	}

	public async Task<List<Receipt>> GetByMoneyRangeAsync(Money minAmount, Money maxAmount, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> entities = await _context.Receipts
			.Where(e => (e.TaxAmount + e.Transactions.Sum(t => t.Amount)).Between(minAmount.Amount, maxAmount.Amount))
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Receipt>).ToList();
	}

	public async Task<List<Receipt>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> entities = await _context.Receipts
			.Where(e => e.Date.Between(startDate, endDate))
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Receipt>).ToList();
	}

	public async Task<List<Receipt>> CreateAsync(List<Receipt> receipts, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> createdEntities = [];

		foreach (ReceiptEntity entity in receipts.Select(_mapper.Map<ReceiptEntity>).ToList())
		{
			EntityEntry<ReceiptEntity> entityEntry = await _context.Receipts.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		return createdEntities.Select(_mapper.Map<Receipt>).ToList();
	}

	public async Task<bool> UpdateAsync(List<Receipt> receipts, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> newEntities = receipts.Select(_mapper.Map<ReceiptEntity>).ToList();

		foreach (ReceiptEntity newEntity in newEntities)
		{
			ReceiptEntity existingEntity = await _context.Receipts.SingleAsync(e => e.Id == newEntity.Id, cancellationToken);
			existingEntity.Description = newEntity.Description;
			existingEntity.Location = newEntity.Location;
			existingEntity.Date = newEntity.Date;
			existingEntity.TaxAmount = newEntity.TaxAmount;
		}

		return true;
	}

	public async Task<bool> DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> entities = await _context.Receipts.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
		_context.Receipts.RemoveRange(entities);

		return true;
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await _context.Receipts.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await _context.Receipts.CountAsync(cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await _context.SaveChangesAsync(cancellationToken);
	}
}