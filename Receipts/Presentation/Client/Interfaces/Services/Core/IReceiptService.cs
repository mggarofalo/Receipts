using Shared.ViewModels.Core;

namespace Client.Interfaces.Services.Core;

public interface IReceiptService
{
	Task<List<ReceiptVM>?> CreateReceiptsAsync(List<ReceiptVM> models, CancellationToken cancellationToken = default);
	Task<ReceiptVM?> GetReceiptByIdAsync(Guid id, CancellationToken cancellationToken = default);
	Task<List<ReceiptVM>?> GetAllReceiptsAsync(CancellationToken cancellationToken = default);
	Task<bool> UpdateReceiptsAsync(List<ReceiptVM> models, CancellationToken cancellationToken = default);
	Task<bool> DeleteReceiptsAsync(List<Guid> ids, CancellationToken cancellationToken = default);
}
