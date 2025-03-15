using Application.Commands.Transaction.Delete;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Transaction;

public class DeleteTransactionCommandTests : ICommandTests<Guid>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Guid> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new DeleteTransactionCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Guid> items = [];
		Assert.Throws<ArgumentException>(() => new DeleteTransactionCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Guid> items = TransactionGenerator.GenerateList(2).Select(t => t.Id!.Value).ToList();
		DeleteTransactionCommand command = new(items);
		Assert.Equal(items, command.Ids);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Guid> items = TransactionGenerator.GenerateList(2).Select(t => t.Id!.Value).ToList();
		DeleteTransactionCommand command = new(items);
		Assert.IsAssignableFrom<IReadOnlyList<Guid>>(command.Ids);
		Assert.NotSame(items, command.Ids);
	}
}
