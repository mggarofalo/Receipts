using Application.Interfaces;

namespace Application.Commands.Receipt;

public record UpdateReceiptCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.Receipt> Receipts { get; }
	public const string ReceiptsCannotBeEmptyExceptionMessage = "Receipts list cannot be empty.";

	public UpdateReceiptCommand(List<Domain.Core.Receipt> receipts)
	{
		ArgumentNullException.ThrowIfNull(receipts);

		if (receipts.Count == 0)
		{
			throw new ArgumentException(ReceiptsCannotBeEmptyExceptionMessage, nameof(receipts));
		}

		Receipts = receipts.AsReadOnly();
	}
}
