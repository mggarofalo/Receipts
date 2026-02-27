using Application.Interfaces.Services;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Mapping;

namespace Infrastructure.Services;

public class CategoryService(ICategoryRepository repository, CategoryMapper mapper) : ICategoryService
{
	public async Task<List<Category>> CreateAsync(List<Category> models, CancellationToken cancellationToken)
	{
		List<CategoryEntity> categoryEntities = [.. models.Select(mapper.ToEntity)];
		List<CategoryEntity> createdCategoryEntities = await repository.CreateAsync(categoryEntities, cancellationToken);
		return [.. createdCategoryEntities.Select(mapper.ToDomain)];
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<List<Category>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<CategoryEntity> categoryEntities = await repository.GetAllAsync(cancellationToken);
		return [.. categoryEntities.Select(mapper.ToDomain)];
	}

	public async Task<List<Category>> GetDeletedAsync(CancellationToken cancellationToken)
	{
		List<CategoryEntity> categoryEntities = await repository.GetDeletedAsync(cancellationToken);
		return [.. categoryEntities.Select(mapper.ToDomain)];
	}

	public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		CategoryEntity? categoryEntity = await repository.GetByIdAsync(id, cancellationToken);
		return categoryEntity == null ? null : mapper.ToDomain(categoryEntity);
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await repository.GetCountAsync(cancellationToken);
	}

	public async Task UpdateAsync(List<Category> models, CancellationToken cancellationToken)
	{
		List<CategoryEntity> categoryEntities = [.. models.Select(mapper.ToEntity)];
		await repository.UpdateAsync(categoryEntities, cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.RestoreAsync(id, cancellationToken);
	}
}
