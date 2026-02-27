using Application.Interfaces.Services;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Mapping;

namespace Infrastructure.Services;

public class SubcategoryService(ISubcategoryRepository repository, SubcategoryMapper mapper) : ISubcategoryService
{
	public async Task<List<Subcategory>> CreateAsync(List<Subcategory> models, CancellationToken cancellationToken)
	{
		List<SubcategoryEntity> subcategoryEntities = [.. models.Select(mapper.ToEntity)];
		List<SubcategoryEntity> createdSubcategoryEntities = await repository.CreateAsync(subcategoryEntities, cancellationToken);
		return [.. createdSubcategoryEntities.Select(mapper.ToDomain)];
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<List<Subcategory>> GetAllAsync(CancellationToken cancellationToken)
	{
		List<SubcategoryEntity> subcategoryEntities = await repository.GetAllAsync(cancellationToken);
		return [.. subcategoryEntities.Select(mapper.ToDomain)];
	}

	public async Task<List<Subcategory>> GetDeletedAsync(CancellationToken cancellationToken)
	{
		List<SubcategoryEntity> subcategoryEntities = await repository.GetDeletedAsync(cancellationToken);
		return [.. subcategoryEntities.Select(mapper.ToDomain)];
	}

	public async Task<Subcategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		SubcategoryEntity? subcategoryEntity = await repository.GetByIdAsync(id, cancellationToken);
		return subcategoryEntity == null ? null : mapper.ToDomain(subcategoryEntity);
	}

	public async Task<List<Subcategory>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken)
	{
		List<SubcategoryEntity> subcategoryEntities = await repository.GetByCategoryIdAsync(categoryId, cancellationToken);
		return [.. subcategoryEntities.Select(mapper.ToDomain)];
	}

	public async Task<int> GetCountAsync(CancellationToken cancellationToken)
	{
		return await repository.GetCountAsync(cancellationToken);
	}

	public async Task UpdateAsync(List<Subcategory> models, CancellationToken cancellationToken)
	{
		List<SubcategoryEntity> subcategoryEntities = [.. models.Select(mapper.ToEntity)];
		await repository.UpdateAsync(subcategoryEntities, cancellationToken);
	}

	public async Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.RestoreAsync(id, cancellationToken);
	}
}
