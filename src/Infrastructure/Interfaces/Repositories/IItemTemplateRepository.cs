using Infrastructure.Entities.Core;

namespace Infrastructure.Interfaces.Repositories;

public interface IItemTemplateRepository
{
	Task<ItemTemplateEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
	Task<List<ItemTemplateEntity>> GetAllAsync(CancellationToken cancellationToken);
	Task<List<ItemTemplateEntity>> GetDeletedAsync(CancellationToken cancellationToken);
	Task<List<ItemTemplateEntity>> CreateAsync(List<ItemTemplateEntity> entities, CancellationToken cancellationToken);
	Task UpdateAsync(List<ItemTemplateEntity> entities, CancellationToken cancellationToken);
	Task DeleteAsync(List<Guid> ids, CancellationToken cancellationToken);
	Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
	Task<int> GetCountAsync(CancellationToken cancellationToken);
	Task<bool> RestoreAsync(Guid id, CancellationToken cancellationToken);
}
