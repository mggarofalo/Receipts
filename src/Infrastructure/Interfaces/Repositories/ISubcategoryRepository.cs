using Application.Models;
using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface ISubcategoryRepository
{
	Task<SubcategoryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<SubcategoryEntity>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<List<SubcategoryEntity>> GetDeletedAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<int> GetDeletedCountAsync(CancellationToken cancellationToken);
	Task<List<SubcategoryEntity>> GetByCategoryIdAsync(Guid categoryId, int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<int> GetByCategoryIdCountAsync(Guid categoryId, CancellationToken cancellationToken);
	Task<List<SubcategoryEntity>> CreateAsync(List<SubcategoryEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<SubcategoryEntity> entities, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetReceiptItemCountBySubcategoryNameAsync(string subcategoryName, CancellationToken cancellationToken);
}
