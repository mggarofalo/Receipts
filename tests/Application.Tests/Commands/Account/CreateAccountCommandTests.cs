using Application.Commands.Account;
using SampleData.Domain.Core;

namespace Application.Tests.Commands.Account;

public class CreateAccountCommandTests : ICommandTests<Domain.Core.Account>
{
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
		List<Domain.Core.Account> items = AccountGenerator.GenerateList(2);
		CreateAccountCommand command = new(items);
		Assert.Equal(items, command.Accounts);
	}

	[Fact]
	public void Items_ShouldBeImmutable()
	{
		List<Domain.Core.Account> items = AccountGenerator.GenerateList(2);
		CreateAccountCommand command = new(items);
		Assert.True(command.Accounts is not null);
	}
}
