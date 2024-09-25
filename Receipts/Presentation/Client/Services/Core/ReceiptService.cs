using Client.Interfaces.Services.Core;
using Shared.HttpClientApiExtensions.Core;
using Shared.ViewModels.Core;

namespace Client.Services.Core;

public class ReceiptService(HttpClient httpClient) : IReceiptService
{
	public async Task<List<ReceiptVM>?> CreateReceiptsAsync(List<ReceiptVM> models, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.CreateReceiptsAsync(models, cancellationToken);
	}

	public async Task<ReceiptVM?> GetReceiptByIdAsync(Guid id, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetReceiptByIdAsync(id, cancellationToken);
	}

	public async Task<List<ReceiptVM>?> GetAllReceiptsAsync(CancellationToken cancellationToken = default)
	{
		return await httpClient
			.GetAllReceiptsAsync(cancellationToken);
	}

	public async Task<bool> UpdateReceiptsAsync(List<ReceiptVM> models, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.UpdateReceiptsAsync(models, cancellationToken);
	}

	public async Task<bool> DeleteReceiptsAsync(List<Guid> ids, CancellationToken cancellationToken = default)
	{
		return await httpClient
			.DeleteReceiptsAsync(ids, cancellationToken);
	}
}
