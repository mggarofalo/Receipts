using Application.Commands.Account;

namespace Application.Tests.Commands.Account;

public class CreateAccountCommandTests : ICommandTests<Domain.Core.Account>
{
	public List<Domain.Core.Account> GenerateItemsForTest()
	{
		return
		[
			new(null, "Test Account 1", "Test Account 1", true),
			new(null, "Test Account 2", "Test Account 2", true)
		];
	}

	[Fact]
	public void Command_WithNullItems_ThrowsArgumentNullException()
	{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
		List<Domain.Core.Account> items = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
		Assert.Throws<ArgumentNullException>(() => new CreateAccountCommand(items));
#pragma warning restore CS8604 // Possible null reference argument.
	}

	[Fact]
	public void Command_WithEmptyItems_ThrowsArgumentException()
	{
		List<Domain.Core.Account> items = [];
		Assert.Throws<ArgumentException>(() => new CreateAccountCommand(items));
	}

	[Fact]
	public void Command_WithValidItems_ReturnsValidCommand()
	{
		List<Domain.Core.Account> items = GenerateItemsForTest();
		CreateAccountCommand command = new(items);
		Assert.Equal(items, command.Accounts);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Account> items = GenerateItemsForTest();
		CreateAccountCommand command = new(items);
		Assert.True(command.Accounts is IReadOnlyList<Domain.Core.Account>);
	}
}
