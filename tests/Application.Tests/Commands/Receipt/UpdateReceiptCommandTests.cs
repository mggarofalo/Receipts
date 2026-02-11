using Application.Commands.Receipt.Update;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Receipt;

public class UpdateReceiptCommandTests : ICommandTests<Domain.Core.Receipt>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Receipt> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new UpdateReceiptCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.Receipt> items = [];
		Assert.Throws<ArgumentException>(() => new UpdateReceiptCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.Receipt> items = ReceiptGenerator.GenerateList(2);
		UpdateReceiptCommand command = new(items);
		command.Receipts.Should().BeEquivalentTo(items);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Receipt> items = ReceiptGenerator.GenerateList(2);
		UpdateReceiptCommand command = new(items);
		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.Receipt>>(command.Receipts);
		Assert.NotSame(items, command.Receipts);
	}
}
