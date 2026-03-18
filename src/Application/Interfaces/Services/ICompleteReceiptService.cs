using Application.Commands.Receipt.CreateComplete;
using Domain.Core;

namespace Application.Interfaces.Services;

public interface ICompleteReceiptService
{
	Task<CreateCompleteReceiptResult> CreateCompleteReceiptAsync(
		Receipt receipt,
		List<Transaction> transactions,
		List<ReceiptItem> items,
		CancellationToken cancellationToken);
}
