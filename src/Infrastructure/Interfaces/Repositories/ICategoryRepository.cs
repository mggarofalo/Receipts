using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface ICategoryRepository
{
	Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<CategoryEntity>> GetAllAsync(int offset, int limit, CancellationToken cancellationToken);
	Task<List<CategoryEntity>> GetDeletedAsync(int offset, int limit, CancellationToken cancellationToken);
	Task<int> GetDeletedCountAsync(CancellationToken cancellationToken);
	Task<List<CategoryEntity>> CreateAsync(List<CategoryEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<CategoryEntity> entities, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
}
