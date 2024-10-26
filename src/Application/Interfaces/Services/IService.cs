namespace Application.Interfaces.Services;

public interface IService<T>
{
	Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<T>> GetAllAsync(CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
}