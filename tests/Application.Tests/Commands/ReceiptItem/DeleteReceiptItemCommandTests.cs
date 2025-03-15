using Application.Commands.ReceiptItem.Delete;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.ReceiptItem;

public class DeleteReceiptItemCommandTests : ICommandTests<Guid>
{
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
		List<Guid> items = ReceiptItemGenerator.GenerateList(2).Select(ri => ri.Id!.Value).ToList();
		DeleteReceiptItemCommand command = new(items);
		Assert.Equal(items, command.Ids);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Guid> items = ReceiptItemGenerator.GenerateList(2).Select(ri => ri.Id!.Value).ToList();
		DeleteReceiptItemCommand command = new(items);
		Assert.IsAssignableFrom<IReadOnlyList<Guid>>(command.Ids);
		Assert.NotSame(items, command.Ids);
	}
}
