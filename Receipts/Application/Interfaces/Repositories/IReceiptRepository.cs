using Domain.Core;

namespace Application.Interfaces.Repositories;

public interface IReceiptRepository : IRepository<Receipt>
{
	Task<List<Receipt>> CreateAsync(List<Receipt> models, CancellationToken cancellationToken);
	Task UpdateAsync(List<Receipt> models, CancellationToken cancellationToken);
}