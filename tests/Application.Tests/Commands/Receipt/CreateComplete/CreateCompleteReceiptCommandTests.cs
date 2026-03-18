using Application.Commands.Receipt.CreateComplete;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Receipt.CreateComplete;

public class CreateCompleteReceiptCommandTests
{
	[Fact]
	public void Command_WithNullReceipt_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		Domain.Core.Receipt receipt = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateCompleteReceiptCommand(receipt, [], []));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithNullTransactions_ThrowsArgumentNullException()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Transaction> transactions = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateCompleteReceiptCommand(receipt, transactions, []));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.ReceiptItem> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateCompleteReceiptCommand(receipt, [], items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithValidInputs_ReturnsValidCommand()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.Transaction> transactions = TransactionGenerator.GenerateList(2);
		List<Domain.Core.ReceiptItem> items = ReceiptItemGenerator.GenerateList(2);

		CreateCompleteReceiptCommand command = new(receipt, transactions, items);

		command.Receipt.Should().Be(receipt);
		command.Transactions.Should().BeEquivalentTo(transactions);
		command.Items.Should().BeEquivalentTo(items);
	}

	[Fact]
	public void Command_WithEmptyLists_Succeeds()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();

		CreateCompleteReceiptCommand command = new(receipt, [], []);

		command.Receipt.Should().Be(receipt);
		command.Transactions.Should().BeEmpty();
		command.Items.Should().BeEmpty();
	}

	[Fact]
	public void Transactions_ShouldBeImmutable()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.Transaction> transactions = TransactionGenerator.GenerateList(2);

		CreateCompleteReceiptCommand command = new(receipt, transactions, []);

		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.Transaction>>(command.Transactions);
		Assert.NotSame(transactions, command.Transactions);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		Domain.Core.Receipt receipt = ReceiptGenerator.Generate();
		List<Domain.Core.ReceiptItem> items = ReceiptItemGenerator.GenerateList(2);

		CreateCompleteReceiptCommand command = new(receipt, [], items);

		Assert.IsAssignableFrom<IReadOnlyList<Domain.Core.ReceiptItem>>(command.Items);
		Assert.NotSame(items, command.Items);
	}
}
