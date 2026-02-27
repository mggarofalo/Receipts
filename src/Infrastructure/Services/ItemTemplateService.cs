using Application.Interfaces.Services;
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

	public async Task<List<ItemTemplate>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<ItemTemplateEntity> entities = await repository.GetAllAsync(cancellationToken);
		return [.. entities.Select(mapper.ToDomain)];
	}

	public async Task<List<ItemTemplate>> GetDeletedAsync(CancellationToken cancellationToken)
	{
		List<ItemTemplateEntity> entities = await repository.GetDeletedAsync(cancellationToken);
		return [.. entities.Select(mapper.ToDomain)];
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
