using Application.Interfaces;

namespace Application.Commands.ReceiptItem;

public record CreateReceiptItemCommand : ICommand<List<Domain.Core.ReceiptItem>>
{
	public IReadOnlyList<Domain.Core.ReceiptItem> ReceiptItems { get; }
	public Guid ReceiptId { get; }
	public const string ReceiptItemsListCannotBeEmpty = "Receipt items list cannot be empty.";

	public CreateReceiptItemCommand(List<Domain.Core.ReceiptItem> receiptItems, Guid receiptId)
	{
		ArgumentNullException.ThrowIfNull(receiptItems);
		ArgumentNullException.ThrowIfNull(receiptId);

		if (receiptItems.Count == 0)
		{
			throw new ArgumentException(ReceiptItemsListCannotBeEmpty, nameof(receiptItems));
		}

		ReceiptItems = receiptItems.AsReadOnly();
		ReceiptId = receiptId;
	}
}
