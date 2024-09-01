using Application.Commands.Transaction;
using Domain;

namespace Application.Tests.Commands.Transaction;

public class CreateTransactionCommandTests : ICommandTests<Domain.Core.Transaction>
{
	public List<Domain.Core.Transaction> GenerateItemsForTest()
	{
		return
		[
			new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(100), DateOnly.FromDateTime(DateTime.Now)),
			new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), new Money(200), DateOnly.FromDateTime(DateTime.Now))
		];
	}

	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Transaction> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateTransactionCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.Transaction> items = [];
		Assert.Throws<ArgumentException>(() => new CreateTransactionCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.Transaction> items = GenerateItemsForTest();
		CreateTransactionCommand command = new(items);
		Assert.Equal(items, command.Transactions);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Transaction> items = GenerateItemsForTest();
		CreateTransactionCommand command = new(items);
		Assert.True(command.Transactions is IReadOnlyList<Domain.Core.Transaction>);
	}
}