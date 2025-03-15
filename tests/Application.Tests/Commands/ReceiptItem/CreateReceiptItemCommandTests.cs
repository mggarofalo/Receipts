using Application.Commands.ReceiptItem.Create;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.ReceiptItem;

public class CreateReceiptItemCommandTests : ICommandTests<Domain.Core.ReceiptItem>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.ReceiptItem> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateReceiptItemCommand(items, Guid.NewGuid()));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.ReceiptItem> items = [];
		Assert.Throws<ArgumentException>(() => new CreateReceiptItemCommand(items, Guid.NewGuid()));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.ReceiptItem> items = ReceiptItemGenerator.GenerateList(2);
		CreateReceiptItemCommand command = new(items, Guid.NewGuid());
		Assert.Equal(items, command.ReceiptItems);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.ReceiptItem> items = ReceiptItemGenerator.GenerateList(2);
		CreateReceiptItemCommand command = new(items, Guid.NewGuid());
		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.ReceiptItem>>(command.ReceiptItems);
		Assert.NotSame(items, command.ReceiptItems);
	}
}
