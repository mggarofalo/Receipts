using Application.Commands.Adjustment.Update;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Adjustment;

public class UpdateAdjustmentCommandTests : ICommandTests<Domain.Core.Adjustment>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Adjustment> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new UpdateAdjustmentCommand(items, Guid.NewGuid()));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.Adjustment> items = [];
		Assert.Throws<ArgumentException>(() => new UpdateAdjustmentCommand(items, Guid.NewGuid()));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		Guid receiptId = Guid.NewGuid();
		List<Domain.Core.Adjustment> items = AdjustmentGenerator.GenerateList(2);
		UpdateAdjustmentCommand command = new(items, receiptId);
		command.Adjustments.Should().BeEquivalentTo(items);
		Assert.Equal(receiptId, command.ReceiptId);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Adjustment> items = AdjustmentGenerator.GenerateList(2);
		UpdateAdjustmentCommand command = new(items, Guid.NewGuid());
		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.Adjustment>>(command.Adjustments);
		Assert.NotSame(items, command.Adjustments);
	}
}
