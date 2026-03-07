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

public class SubcategoryService(ISubcategoryRepository repository, SubcategoryMapper mapper) : ISubcategoryService
{
	public async Task<List<Subcategory>> CreateAsync(List<Subcategory> models, CancellationToken cancellationToken)
	{
		try
		{
			List<SubcategoryEntity> subcategoryEntities = [.. models.Select(mapper.ToEntity)];
			List<SubcategoryEntity> createdSubcategoryEntities = await repository.CreateAsync(subcategoryEntities, cancellationToken);
			return [.. createdSubcategoryEntities.Select(mapper.ToDomain)];
		}
		catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
		{
			throw new DuplicateEntityException("A subcategory with this name already exists in this category.", ex);
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

	public async Task<PagedResult<Subcategory>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		int total = await repository.GetCountAsync(cancellationToken);
		List<SubcategoryEntity> entities = await repository.GetAllAsync(offset, limit, sort, cancellationToken);
		List<Subcategory> data = [.. entities.Select(mapper.ToDomain)];
		return new PagedResult<Subcategory>(data, total, offset, limit);
	}

	public async Task<PagedResult<Subcategory>> GetDeletedAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		int total = await repository.GetDeletedCountAsync(cancellationToken);
		List<SubcategoryEntity> entities = await repository.GetDeletedAsync(offset, limit, sort, cancellationToken);
		List<Subcategory> data = [.. entities.Select(mapper.ToDomain)];
		return new PagedResult<Subcategory>(data, total, offset, limit);
	}

	public async Task<Subcategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
	{
		SubcategoryEntity? subcategoryEntity = await repository.GetByIdAsync(id, cancellationToken);
		return subcategoryEntity == null ? null : mapper.ToDomain(subcategoryEntity);
	}

	public async Task<PagedResult<Subcategory>> GetByCategoryIdAsync(Guid categoryId, int offset, int limit, SortParams sort, CancellationToken cancellationToken)
	{
		int total = await repository.GetByCategoryIdCountAsync(categoryId, cancellationToken);
		List<SubcategoryEntity> entities = await repository.GetByCategoryIdAsync(categoryId, offset, limit, sort, cancellationToken);
		List<Subcategory> data = entities.Select(mapper.ToDomain).ToList();
		return new PagedResult<Subcategory>(data, total, offset, limit);
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
