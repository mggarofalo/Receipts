using Application.Interfaces.Services;
using AutoMapper;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;

namespace Infrastructure.Services;

public class ReceiptItemService(IReceiptItemRepository repository, IMapper mapper) : IReceiptItemService
{
	public async Task<List<ReceiptItem>> CreateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> receiptItemEntities = models.Select(mapper.Map<ReceiptItemEntity>).ToList();

		foreach (ReceiptItemEntity entity in receiptItemEntities)
		{
			entity.ReceiptId = receiptId;
		}

		List<ReceiptItemEntity> createdReceiptItemEntities = await repository.CreateAsync(receiptItemEntities, cancellationToken);
		return createdReceiptItemEntities.Select(mapper.Map<ReceiptItem>).ToList();
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<List<ReceiptItem>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> receiptItemEntities = await repository.GetAllAsync(cancellationToken);
		return receiptItemEntities.Select(mapper.Map<ReceiptItem>).ToList();
	}

	public async Task<ReceiptItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		ReceiptItemEntity? receiptItemEntity = await repository.GetByIdAsync(id, cancellationToken);
		return receiptItemEntity == null ? null : mapper.Map<ReceiptItem>(receiptItemEntity);
	}

	public async Task<List<ReceiptItem>?> GetByReceiptIdAsync(Guid receiptId, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity>? receiptItemEntities = await repository.GetByReceiptIdAsync(receiptId, cancellationToken);
		return receiptItemEntities?.Select(mapper.Map<ReceiptItem>).ToList();
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await repository.GetCountAsync(cancellationToken);
	}

	public async Task UpdateAsync(List<ReceiptItem> models, Guid receiptId, CancellationToken cancellationToken)
	{
		List<ReceiptItemEntity> receiptItemEntities = models.Select(mapper.Map<ReceiptItemEntity>).ToList();

		foreach (ReceiptItemEntity entity in receiptItemEntities)
		{
			entity.ReceiptId = receiptId;
		}

		await repository.UpdateAsync(receiptItemEntities, cancellationToken);
	}
}
