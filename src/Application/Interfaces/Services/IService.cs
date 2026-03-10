using Application.Models;

namespace Application.Interfaces.Services;

public interface IService<T>
{
	Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<PagedResult<T>> GetAllAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
}

public interface ISoftDeletableService<T> : IService<T>
{
	Task<PagedResult<T>> GetDeletedAsync(int offset, int limit, SortParams sort, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
}
