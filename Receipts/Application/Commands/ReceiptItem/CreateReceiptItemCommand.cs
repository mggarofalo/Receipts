using Application.Interfaces;

namespace Application.Commands.ReceiptItem;

public record CreateReceiptItemCommand : ICommand<List<Domain.Core.ReceiptItem>>
{
	public IReadOnlyList<Domain.Core.ReceiptItem> ReceiptItems { get; }
	public const string ReceiptItemsCannotBeEmptyExceptionMessage = "Receipt items list cannot be empty.";

	public CreateReceiptItemCommand(List<Domain.Core.ReceiptItem> receiptItems)
	{
		ArgumentNullException.ThrowIfNull(receiptItems);

		if (receiptItems.Count == 0)
		{
			throw new ArgumentException(ReceiptItemsCannotBeEmptyExceptionMessage, nameof(receiptItems));
		}

		ReceiptItems = receiptItems.AsReadOnly();
	}
}
