using Application.Interfaces;

namespace Application.Commands.Adjustment.Create;

public record CreateAdjustmentCommand : ICommand<List<Domain.Core.Adjustment>>
{
	public IReadOnlyList<Domain.Core.Adjustment> Adjustments { get; }
	public Guid ReceiptId { get; }

	public const string AdjustmentsCannotBeEmptyExceptionMessage = "Adjustments list cannot be empty.";

	public CreateAdjustmentCommand(List<Domain.Core.Adjustment> adjustments, Guid receiptId)
	{
		ArgumentNullException.ThrowIfNull(adjustments);

		if (adjustments.Count == 0)
		{
			throw new ArgumentException(AdjustmentsCannotBeEmptyExceptionMessage, nameof(adjustments));
		}

		Adjustments = adjustments.AsReadOnly();
		ReceiptId = receiptId;
	}
}
