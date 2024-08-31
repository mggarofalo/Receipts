namespace Application.Interfaces.Repositories;

public interface IRepository<T>
{
	Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
	Task<List<T>> CreateAsync(List<T> models, CancellationToken cancellationToken);
	Task<bool> UpdateAsync(List<T> models, CancellationToken cancellationToken);
	Task<bool> DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task SaveChangesAsync(CancellationToken cancellationToken);
}