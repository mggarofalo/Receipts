using Application.Commands.Receipt;
using Domain;

namespace Application.Tests.Commands.Receipt;

public class CreateReceiptCommandTests : ICommandTests<Domain.Core.Receipt>
{
	public List<Domain.Core.Receipt> GenerateItemsForTest()
	{
		return
		[
			new(null, "Location 1", DateOnly.FromDateTime(DateTime.Today), new Money(100)),
			new(null, "Location 2", DateOnly.FromDateTime(DateTime.Today), new Money(200))
		];
	}

	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Receipt> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateReceiptCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.Receipt> items = [];
		Assert.Throws<ArgumentException>(() => new CreateReceiptCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.Receipt> items = GenerateItemsForTest();
		CreateReceiptCommand command = new(items);
		Assert.Equal(items, command.Receipts);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Receipt> items = GenerateItemsForTest();
		CreateReceiptCommand command = new(items);
		Assert.True(command.Receipts is IReadOnlyList<Domain.Core.Receipt>);
	}
}
