using Application.Interfaces;
using AutoMapper;
using Common;
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

	public async Task<Receipt> CreateAsync(Receipt receipt, CancellationToken cancellationToken)
	{
		ReceiptEntity entity = _mapper.Map<ReceiptEntity>(receipt);

		await _context.Receipts
			.AddAsync(entity, cancellationToken);

		return _mapper.Map<Receipt>(entity);
	}

	public async Task<bool> UpdateAsync(Receipt receipt, CancellationToken cancellationToken)
	{
		ReceiptEntity newEntity = _mapper.Map<ReceiptEntity>(receipt);

		await _context.Receipts.Where(x => x.Id == receipt.Id).ExecuteUpdateAsync(x => x
			.SetProperty(e => e.Description, newEntity.Description)
			.SetProperty(e => e.Location, newEntity.Location)
			.SetProperty(e => e.Date, newEntity.Date)
			.SetProperty(e => e.TaxAmount, newEntity.TaxAmount), cancellationToken
		);

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

		_ = _context.Receipts.Remove(entity);

		int count = await _context.Receipts.ExecuteDeleteAsync(cancellationToken);

		return count switch
		{
			1 => true,
			0 => false,
			_ => throw new ApplicationException($"{count} Receipt entities deleted with id {id}")
		};
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