using Application.Interfaces;
using AutoMapper;
using Domain;
using Domain.Core;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ReceiptRepository(ApplicationDbContext context, IMapper mapper) : IReceiptRepository
{
	private readonly ApplicationDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		ReceiptEntity? entity = await _context.Receipts
			.FindAsync([id], cancellationToken);

		return _mapper.Map<Receipt?>(entity);
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
			.Where(e => e.TotalAmount >= minAmount.Amount && e.TotalAmount <= maxAmount.Amount)
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Receipt>).ToList();
	}

	public async Task<List<Receipt>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> entities = await _context.Receipts
			.Where(e => e.Date >= startDate && e.Date <= endDate)
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Receipt>).ToList();
	}

	public async Task<Receipt> CreateAsync(Receipt receipt, CancellationToken cancellationToken)
	{
		ReceiptEntity entity = _mapper.Map<ReceiptEntity>(receipt);

		await _context.Receipts
			.AddAsync(entity, cancellationToken);

		return receipt;
	}

	public async Task<bool> UpdateAsync(Receipt receipt, CancellationToken cancellationToken)
	{
		ReceiptEntity newEntity = _mapper.Map<ReceiptEntity>(receipt);
		ReceiptEntity? oldEntity = await _context.Receipts.FindAsync([receipt.Id], cancellationToken);

		if (oldEntity == null)
		{
			return false;
		}

		oldEntity.Location = newEntity.Location;
		oldEntity.Date = newEntity.Date;
		oldEntity.TaxAmount = newEntity.TaxAmount;
		oldEntity.TotalAmount = newEntity.TotalAmount;
		oldEntity.Description = newEntity.Description;

		await SaveChangesAsync(cancellationToken);

		return true;
	}

	public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
	{
		ReceiptEntity? entity = await _context.Receipts.FindAsync([id], cancellationToken);

		if (entity == null)
		{
			return false;
		}

		_context.Receipts.Remove(entity);
		await _context.Receipts.ExecuteDeleteAsync(cancellationToken);

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
