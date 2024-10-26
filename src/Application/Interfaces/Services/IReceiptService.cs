using Domain.Core;

namespace Application.Interfaces.Services;

public interface IReceiptService : IService<Receipt>
{
	Task<List<Receipt>> CreateAsync(List<Receipt> models, CancellationToken cancellationToken);
	Task UpdateAsync(List<Receipt> models, CancellationToken cancellationToken);
}