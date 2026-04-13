using Application.Interfaces.Services;
using Application.Models;
using Domain.Aggregates;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Mapping;

namespace Infrastructure.Services;

public class TransactionService(ITransactionRepository repository, TransactionMapper mapper, CardMapper cardMapper) : ITransactionService
{
	public async Task<List<Transaction>> CreateAsync(List<Transaction> models, Guid receiptId, CancellationToken cancellationToken)
	{
		List<TransactionEntity> transactionEntities = [.. models.Select(mapper.ToEntity)];

		foreach (TransactionEntity entity in transactionEntities)
		{
			entity.ReceiptId = receiptId;
		}

		List<TransactionEntity> createdTransactionEntities = await repository.CreateAsync(transactionEntities, cancellationToken);
		return [.. createdTransactionEntities.Select(mapper.ToDomain)];
	}


	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<PagedResult<Transaction>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		int total = await repository.GetCountAsync(cancellationToken);
		List<TransactionEntity> entities = await repository.GetAllAsync(offset, limit, sort, cancellationToken);
		List<Transaction> data = [.. entities.Select(mapper.ToDomain)];
		return new PagedResult<Transaction>(data, total, offset, limit);
	}

	public async Task<PagedResult<Transaction>> GetDeletedAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		int total = await repository.GetDeletedCountAsync(cancellationToken);
		List<TransactionEntity> entities = await repository.GetDeletedAsync(offset, limit, sort, cancellationToken);
		List<Transaction> data = [.. entities.Select(mapper.ToDomain)];
		return new PagedResult<Transaction>(data, total, offset, limit);
	}

	public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		TransactionEntity? transactionEntity = await repository.GetByIdAsync(id, cancellationToken);
		return transactionEntity == null ? null : mapper.ToDomain(transactionEntity);
	}

	public async Task<PagedResult<Transaction>> GetByReceiptIdAsync(Guid receiptId, int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		int total = await repository.GetByReceiptIdCountAsync(receiptId, cancellationToken);
		List<TransactionEntity> entities = await repository.GetByReceiptIdAsync(receiptId, offset, limit, sort, cancellationToken);
		List<Transaction> data = entities.Select(mapper.ToDomain).ToList();
		return new PagedResult<Transaction>(data, total, offset, limit);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await repository.GetCountAsync(cancellationToken);
	}

	public async Task UpdateAsync(List<Transaction> models, Guid receiptId, CancellationToken cancellationToken)
	{
		List<TransactionEntity> transactionEntities = [.. models.Select(mapper.ToEntity)];

		foreach (TransactionEntity entity in transactionEntities)
		{
			entity.ReceiptId = receiptId;
		}

		await repository.UpdateAsync(transactionEntities, cancellationToken);
	}

	public async Task<List<TransactionAccount>> GetTransactionAccountsByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		List<TransactionEntity> entities = await repository.GetWithAccountByReceiptIdAsync(receiptId, cancellationToken);
		return
		[
			.. entities
				.Where(e => e.Account != null)
				.Select(e => new TransactionAccount
				{
					Transaction = mapper.ToDomain(e),
					Account = cardMapper.ToDomain(e.Account!)
				})
		];
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.RestoreAsync(id, cancellationToken);
	}
}

