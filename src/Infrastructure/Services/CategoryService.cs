using Application.Exceptions;
using Application.Interfaces.Services;
using Application.Models;
using Domain.Core;
using Infrastructure.Entities.Core;
using Infrastructure.Interfaces.Repositories;
using Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Infrastructure.Services;

public class CategoryService(ICategoryRepository repository, CategoryMapper mapper) : ICategoryService
{
	public async Task<List<Category>> CreateAsync(List<Category> models, CancellationToken cancellationToken)
	{
		try
		{
			List<CategoryEntity> categoryEntities = [.. models.Select(mapper.ToEntity)];
			List<CategoryEntity> createdCategoryEntities = await repository.CreateAsync(categoryEntities, cancellationToken);
			return [.. createdCategoryEntities.Select(mapper.ToDomain)];
		}
		catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
		{
			throw new DuplicateEntityException("A category with this name already exists.", ex);
		}
	}

	public async Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken)
	{
		await repository.DeleteAsync(ids, cancellationToken);
	}

	public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
	{
		return await repository.ExistsAsync(id, cancellationToken);
	}

	public async Task<PagedResult<Category>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		int total = await repository.GetCountAsync(cancellationToken);
		List<CategoryEntity> entities = await repository.GetAllAsync(offset, limit, sort, cancellationToken);
		List<Category> data = [.. entities.Select(mapper.ToDomain)];
		return new PagedResult<Category>(data, total, offset, limit);
	}

	public async Task<PagedResult<Category>> GetDeletedAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		int total = await repository.GetDeletedCountAsync(cancellationToken);
		List<CategoryEntity> entities = await repository.GetDeletedAsync(offset, limit, sort, cancellationToken);
		List<Category> data = [.. entities.Select(mapper.ToDomain)];
		return new PagedResult<Category>(data, total, offset, limit);
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
