using Application.Commands.ReceiptItem;

namespace Application.Tests.Commands.ReceiptItem;

public class DeleteReceiptItemCommandTests : ICommandTests<Guid>
{
	public List<Guid> GenerateItemsForTest()
	{
		return
		[
			Guid.NewGuid(),
			Guid.NewGuid()
		];
	}

	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Guid> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new DeleteReceiptItemCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Guid> items = [];
		Assert.Throws<ArgumentException>(() => new DeleteReceiptItemCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Guid> items = GenerateItemsForTest();
		DeleteReceiptItemCommand command = new(items);
		Assert.Equal(items, command.Ids);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Guid> items = GenerateItemsForTest();
		DeleteReceiptItemCommand command = new(items);
		Assert.True(command.Ids is IReadOnlyList<Guid>);
	}
}
