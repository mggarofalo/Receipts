using Application.Models;
using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface ICategoryRepository
{
	Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<CategoryEntity>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<List<CategoryEntity>> CreateAsync(List<CategoryEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<CategoryEntity> entities, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task DeleteAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetSubcategoryCountAsync(Guid categoryId, CancellationToken cancellationToken);
	Task<int> GetReceiptItemCountByCategoryNameAsync(string categoryName, CancellationToken cancellationToken);
}
