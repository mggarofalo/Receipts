using Application.Models;
using Domain.Core;

namespace Application.Interfaces.Services;

public interface IReceiptService : ISoftDeletableService<Receipt>
{
	Task<PagedResult<Receipt>> GetAllAsync(int offset, int limit, SortParams sort, Guid? accountId, Guid? cardId, string? q, CancellationToken cancellationToken);
	Task<List<Receipt>> CreateAsync(List<Receipt> models, CancellationToken cancellationToken);
	Task UpdateAsync(List<Receipt> models, CancellationToken cancellationToken);
	Task UpdateOriginalImagePathAsync(Guid receiptId, string originalImagePath, CancellationToken cancellationToken);
	Task<List<string>> GetDistinctLocationsAsync(string? query, int limit, CancellationToken cancellationToken);
}
