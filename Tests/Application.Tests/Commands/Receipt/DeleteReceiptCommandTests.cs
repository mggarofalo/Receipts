using Application.Commands.Receipt;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Receipt;

public class DeleteReceiptCommandTests : ICommandTests<Guid>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Guid> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new DeleteReceiptCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Guid> items = [];
		Assert.Throws<ArgumentException>(() => new DeleteReceiptCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Guid> items = ReceiptGenerator.GenerateList(2).Select(r => r.Id!.Value).ToList();
		DeleteReceiptCommand command = new(items);
		Assert.Equal(items, command.Ids);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Guid> items = ReceiptGenerator.GenerateList(2).Select(r => r.Id!.Value).ToList();
		DeleteReceiptCommand command = new(items);
		Assert.True(command.Ids is not null);
	}
}
