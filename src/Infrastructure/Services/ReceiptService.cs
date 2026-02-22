using Application.Interfaces.Services;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Mapping;

namespace Infrastructure.Services;

public class ReceiptService(IReceiptRepository repository, ReceiptMapper mapper) : IReceiptService
{
	public async Task<List<Receipt>> CreateAsync(List<Receipt> models, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> receiptEntities = [.. models.Select(mapper.ToEntity)];
		List<ReceiptEntity> createdReceiptEntities = await repository.CreateAsync(receiptEntities, cancellationToken);
		return [.. createdReceiptEntities.Select(mapper.ToDomain)];
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<List<Receipt>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<ReceiptEntity> receiptEntities = await repository.GetAllAsync(cancellationToken);
		return [.. receiptEntities.Select(mapper.ToDomain)];
	}

	public async Task<List<Receipt>> GetDeletedAsync(CancellationToken cancellationToken)
	{
		List<ReceiptEntity> receiptEntities = await repository.GetDeletedAsync(cancellationToken);
		return [.. receiptEntities.Select(mapper.ToDomain)];
	}

	public async Task<Receipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		ReceiptEntity? receiptEntity = await repository.GetByIdAsync(id, cancellationToken);
		return receiptEntity == null ? null : mapper.ToDomain(receiptEntity);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await repository.GetCountAsync(cancellationToken);
	}

	public async Task UpdateAsync(List<Receipt> models, CancellationToken cancellationToken)
	{
		List<ReceiptEntity> receiptEntities = [.. models.Select(mapper.ToEntity)];
		await repository.UpdateAsync(receiptEntities, cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.RestoreAsync(id, cancellationToken);
	}
}
