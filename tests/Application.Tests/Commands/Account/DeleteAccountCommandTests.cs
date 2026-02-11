using Application.Commands.Account.Delete;
using FluentAssertions;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Account;

public class DeleteAccountCommandTests : ICommandTests<Guid>
{
	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Guid> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new DeleteAccountCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Guid> items = [];
		Assert.Throws<ArgumentException>(() => new DeleteAccountCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Guid> items = AccountGenerator.GenerateList(2).Select(a => a.Id).ToList();
		DeleteAccountCommand command = new(items);
		command.Ids.Should().BeEquivalentTo(items);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Guid> items = AccountGenerator.GenerateList(2).Select(a => a.Id).ToList();
		DeleteAccountCommand command = new(items);
		Assert.IsAssignableFrom<IReadOnlyList<Guid>>(command.Ids);
		Assert.NotSame(items, command.Ids);
	}
}
