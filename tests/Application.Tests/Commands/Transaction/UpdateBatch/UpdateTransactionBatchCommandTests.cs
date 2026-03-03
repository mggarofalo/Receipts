using Application.Commands.Transaction.UpdateBatch;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Transaction.UpdateBatch;

public class UpdateTransactionBatchCommandTests : ICommandTests<Domain.Core.Transaction>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Transaction> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new UpdateTransactionBatchCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.Transaction> items = [];
		Assert.Throws<ArgumentException>(() => new UpdateTransactionBatchCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.Transaction> items = TransactionGenerator.GenerateList(2);
		UpdateTransactionBatchCommand command = new(items);
		command.Transactions.Should().BeEquivalentTo(items);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Transaction> items = TransactionGenerator.GenerateList(2);
		UpdateTransactionBatchCommand command = new(items);
		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.Transaction>>(command.Transactions);
		Assert.NotSame(items, command.Transactions);
	}
}
