using Application.Interfaces;

namespace Application.Commands.Adjustment.Update;

public record UpdateAdjustmentCommand : ICommand<bool>
{
	public IReadOnlyList<Domain.Core.Adjustment> Adjustments { get; }

	public const string AdjustmentsCannotBeEmptyExceptionMessage = "Adjustments list cannot be empty.";

	public UpdateAdjustmentCommand(List<Domain.Core.Adjustment> adjustments)
	{
		ArgumentNullException.ThrowIfNull(adjustments);

		if (adjustments.Count == 0)
		{
			throw new ArgumentException(AdjustmentsCannotBeEmptyExceptionMessage, nameof(adjustments));
		}

		Adjustments = adjustments.AsReadOnly();
	}
}
