using Application.Interfaces.Services;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Mapping;

namespace Infrastructure.Services;

public class AdjustmentService(IAdjustmentRepository repository, AdjustmentMapper mapper) : IAdjustmentService
{
	public async Task<List<Adjustment>> CreateAsync(List<Adjustment> models, Guid receiptId, CancellationToken cancellationToken)
	{
		List<AdjustmentEntity> adjustmentEntities = [.. models.Select(mapper.ToEntity)];

		foreach (AdjustmentEntity entity in adjustmentEntities)
		{
			entity.ReceiptId = receiptId;
		}

		List<AdjustmentEntity> createdAdjustmentEntities = await repository.CreateAsync(adjustmentEntities, cancellationToken);
		return [.. createdAdjustmentEntities.Select(mapper.ToDomain)];
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<List<Adjustment>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<AdjustmentEntity> adjustmentEntities = await repository.GetAllAsync(cancellationToken);
		return [.. adjustmentEntities.Select(mapper.ToDomain)];
	}

	public async Task<List<Adjustment>> GetDeletedAsync(CancellationToken cancellationToken)
	{
		List<AdjustmentEntity> adjustmentEntities = await repository.GetDeletedAsync(cancellationToken);
		return [.. adjustmentEntities.Select(mapper.ToDomain)];
	}

	public async Task<Adjustment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		AdjustmentEntity? adjustmentEntity = await repository.GetByIdAsync(id, cancellationToken);
		return adjustmentEntity == null ? null : mapper.ToDomain(adjustmentEntity);
	}

	public async Task<List<Adjustment>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		List<AdjustmentEntity>? adjustmentEntities = await repository.GetByReceiptIdAsync(receiptId, cancellationToken);
		return adjustmentEntities?.Select(mapper.ToDomain).ToList();
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await repository.GetCountAsync(cancellationToken);
	}

	public async Task UpdateAsync(List<Adjustment> models, Guid receiptId, CancellationToken cancellationToken)
	{
		List<AdjustmentEntity> adjustmentEntities = [.. models.Select(mapper.ToEntity)];

		foreach (AdjustmentEntity entity in adjustmentEntities)
		{
			entity.ReceiptId = receiptId;
		}

		await repository.UpdateAsync(adjustmentEntities, cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.RestoreAsync(id, cancellationToken);
	}
}
