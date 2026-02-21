using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface IReceiptRepository
{
	Task<ReceiptEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<ReceiptEntity>> GetAllAsync(CancellationToken cancellationToken);
	Task<List<ReceiptEntity>> CreateAsync(List<ReceiptEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<ReceiptEntity> entities, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
}
