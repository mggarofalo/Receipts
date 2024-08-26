using Application.Interfaces;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Repositories;

public class AccountRepository(ApplicationDbContext context, IMapper mapper) : IAccountRepository
{
	private readonly ApplicationDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		AccountEntity? entity = await _context.Accounts
			.FindAsync([id], cancellationToken);

		return _mapper.Map<Account>(entity);
	}

	public async Task<List<Account>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<AccountEntity> entities = await _context.Accounts
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Account>).ToList();
	}

	public async Task<List<Account>> CreateAsync(List<Account> models, CancellationToken cancellationToken)
	{
		List<AccountEntity> createdEntities = [];

		foreach (AccountEntity entity in models.Select(_mapper.Map<AccountEntity>).ToList())
		{
			EntityEntry<AccountEntity> entityEntry = await _context.Accounts.AddAsync(entity, cancellationToken);
			createdEntities.Add(entityEntry.Entity);
		}

		return createdEntities.Select(_mapper.Map<Account>).ToList();
	}

	public async Task<bool> UpdateAsync(List<Account> models, CancellationToken cancellationToken)
	{
		List<AccountEntity> newEntities = models.Select(_mapper.Map<AccountEntity>).ToList();

		foreach (AccountEntity newEntity in newEntities)
		{
			AccountEntity existingEntity = await _context.Accounts.SingleAsync(e => e.Id == newEntity.Id, cancellationToken);
			existingEntity.AccountCode = newEntity.AccountCode;
			existingEntity.Name = newEntity.Name;
			existingEntity.IsActive = newEntity.IsActive;
		}

		return true;
	}

	public async Task<bool> DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		List<AccountEntity> entities = await _context.Accounts.Where(e => ids.Contains(e.Id)).ToListAsync(cancellationToken);
		_context.Accounts.RemoveRange(entities);

		return true;
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await _context.Accounts.AnyAsync(e => e.Id == id, cancellationToken);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await _context.Accounts.CountAsync(cancellationToken);
	}

	public async Task SaveChangesAsync(CancellationToken cancellationToken)
	{
		await _context.SaveChangesAsync(cancellationToken);
	}
}