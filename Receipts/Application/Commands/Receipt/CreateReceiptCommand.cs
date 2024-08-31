using Application.Interfaces;

namespace Application.Commands.Receipt;

public record CreateReceiptCommand : ICommand<List<Domain.Core.Receipt>>
{
	public IReadOnlyList<Domain.Core.Receipt> Receipts { get; }
	public const string ReceiptsCannotBeEmptyExceptionMessage = "Receipts list cannot be empty.";

	public CreateReceiptCommand(List<Domain.Core.Receipt> receipts)
	{
		ArgumentNullException.ThrowIfNull(receipts);

		if (receipts.Count == 0)
		{
			throw new ArgumentException(ReceiptsCannotBeEmptyExceptionMessage, nameof(receipts));
		}

		Receipts = receipts.AsReadOnly();
	}
}
