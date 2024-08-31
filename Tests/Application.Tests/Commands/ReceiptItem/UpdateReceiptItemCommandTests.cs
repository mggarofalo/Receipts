using Application.Commands.ReceiptItem;
using Domain;

namespace Application.Tests.Commands.ReceiptItem;

public class UpdateReceiptItemCommandTests : ICommandTests<Domain.Core.ReceiptItem>
{
	public List<Domain.Core.ReceiptItem> GenerateItemsForTest()
	{
		return
		[
			new(Guid.NewGuid(), "Item 1", "Description 1", 1, new Money(10), new Money(10), "Category 1", "Subcategory 1"),
			new(Guid.NewGuid(), "Item 2", "Description 2", 2, new Money(2), new Money(4), "Category 2", "Subcategory 2")
		];
	}

	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.ReceiptItem> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new UpdateReceiptItemCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.ReceiptItem> items = [];
		Assert.Throws<ArgumentException>(() => new UpdateReceiptItemCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.ReceiptItem> items = GenerateItemsForTest();
		UpdateReceiptItemCommand command = new(items);
		Assert.Equal(items, command.ReceiptItems);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.ReceiptItem> items = GenerateItemsForTest();
		UpdateReceiptItemCommand command = new(items);
		Assert.True(command.ReceiptItems is IReadOnlyList<Domain.Core.ReceiptItem>);
	}
}
