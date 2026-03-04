using Application.Interfaces.Services;
using Application.Models;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Mapping;

namespace Infrastructure.Services;

public class ItemTemplateService(IItemTemplateRepository repository, ItemTemplateMapper mapper) : IItemTemplateService
{
	public async Task<List<ItemTemplate>> CreateAsync(List<ItemTemplate> models, CancellationToken cancellationToken)
	{
		List<ItemTemplateEntity> entities = [.. models.Select(mapper.ToEntity)];
		List<ItemTemplateEntity> createdEntities = await repository.CreateAsync(entities, cancellationToken);
		return [.. createdEntities.Select(mapper.ToDomain)];
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<PagedResult<ItemTemplate>> GetAllAsync(int offset, int limit, CancellationToken cancellationToken)
	{
		int total = await repository.GetCountAsync(cancellationToken);
		List<ItemTemplateEntity> entities = await repository.GetAllAsync(offset, limit, cancellationToken);
		List<ItemTemplate> data = [.. entities.Select(mapper.ToDomain)];
		return new PagedResult<ItemTemplate>(data, total, offset, limit);
	}

	public async Task<PagedResult<ItemTemplate>> GetDeletedAsync(int offset, int limit, CancellationToken cancellationToken)
	{
		int total = await repository.GetDeletedCountAsync(cancellationToken);
		List<ItemTemplateEntity> entities = await repository.GetDeletedAsync(offset, limit, cancellationToken);
		List<ItemTemplate> data = [.. entities.Select(mapper.ToDomain)];
		return new PagedResult<ItemTemplate>(data, total, offset, limit);
	}

	public async Task<ItemTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		ItemTemplateEntity? entity = await repository.GetByIdAsync(id, cancellationToken);
		return entity == null ? null : mapper.ToDomain(entity);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await repository.GetCountAsync(cancellationToken);
	}

	public async Task UpdateAsync(List<ItemTemplate> models, CancellationToken cancellationToken)
	{
		List<ItemTemplateEntity> entities = [.. models.Select(mapper.ToEntity)];
		await repository.UpdateAsync(entities, cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.RestoreAsync(id, cancellationToken);
	}
}
