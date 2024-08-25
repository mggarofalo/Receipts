using Application.Interfaces;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Microsoft.EntityFrameworkCore;

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

	public async Task<List<Account>> GetByAccountCodeAsync(string accountCode, CancellationToken cancellationToken)
	{
		List<AccountEntity> entities = await _context.Accounts
			.Where(e => e.AccountCode == accountCode)
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Account>).ToList();
	}

	public async Task<List<Account>> GetByNameAsync(string name, CancellationToken cancellationToken)
	{
		List<AccountEntity> entities = await _context.Accounts
			.Where(e => e.Name == name)
			.ToListAsync(cancellationToken);

		return entities.Select(_mapper.Map<Account>).ToList();
	}

	public async Task<Account> CreateAsync(Account account, CancellationToken cancellationToken)
	{
		AccountEntity entity = _mapper.Map<AccountEntity>(account);

		await _context.Accounts
			.AddAsync(entity, cancellationToken);

		return _mapper.Map<Account>(entity);
	}

	public async Task<bool> UpdateAsync(Account account, CancellationToken cancellationToken)
	{
		AccountEntity newEntity = _mapper.Map<AccountEntity>(account);

		await _context.Accounts.Where(x => x.Id == account.Id).ExecuteUpdateAsync(x => x
			.SetProperty(e => e.AccountCode, newEntity.AccountCode)
			.SetProperty(e => e.Name, newEntity.Name)
			.SetProperty(e => e.IsActive, newEntity.IsActive), cancellationToken
		);

		await SaveChangesAsync(cancellationToken);

		return true;
	}

	public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
	{
		AccountEntity? entity = await _context.Accounts.FindAsync([id], cancellationToken);

		if (entity == null)
		{
			return false;
		}

		_ = _context.Accounts.Remove(entity);

		int count = await _context.Accounts.ExecuteDeleteAsync(cancellationToken);

		return count switch
		{
			1 => true,
			0 => false,
			_ => throw new ApplicationException($"{count} Account entities deleted with id {id}")
		};
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