using Application.Interfaces.Services;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;

namespace Infrastructure.Services;

public class TransactionService(ITransactionRepository repository, IMapper mapper) : ITransactionService
{
	public async Task<List<Transaction>> CreateAsync(List<Transaction> models, Guid receiptId, Guid accountId, CancellationToken cancellationToken)
	{
		List<TransactionEntity> transactionEntities = models.Select(mapper.Map<TransactionEntity>).ToList();

		foreach (TransactionEntity entity in transactionEntities)
		{
			entity.ReceiptId = receiptId;
			entity.AccountId = accountId;
		}

		List<TransactionEntity> createdTransactionEntities = await repository.CreateAsync(transactionEntities, cancellationToken);
		return createdTransactionEntities.Select(mapper.Map<Transaction>).ToList();
	}


	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<List<Transaction>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<TransactionEntity> transactionEntities = await repository.GetAllAsync(cancellationToken);
		return transactionEntities.Select(mapper.Map<Transaction>).ToList();
	}

	public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		TransactionEntity? transactionEntity = await repository.GetByIdAsync(id, cancellationToken);
		return transactionEntity == null ? null : mapper.Map<Transaction>(transactionEntity);
	}

	public async Task<List<Transaction>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		List<TransactionEntity>? transactionEntities = await repository.GetByReceiptIdAsync(receiptId, cancellationToken);
		return transactionEntities?.Select(mapper.Map<Transaction>).ToList();
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await repository.GetCountAsync(cancellationToken);
	}

	public async Task UpdateAsync(List<Transaction> models, Guid receiptId, Guid accountId, CancellationToken cancellationToken)
	{
		List<TransactionEntity> transactionEntities = models.Select(mapper.Map<TransactionEntity>).ToList();

		foreach (TransactionEntity entity in transactionEntities)
		{
			entity.ReceiptId = receiptId;
			entity.AccountId = accountId;
		}

		await repository.UpdateAsync(transactionEntities, cancellationToken);
	}
}

