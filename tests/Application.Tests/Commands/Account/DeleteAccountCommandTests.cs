using Application.Commands.Account.Delete;
using FluentAssertions;

namespace Application.Tests.Commands.Account;

public class DeleteAccountCommandTests
{
	[Fact]
	public void Command_WithEmptyId_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => new DeleteAccountCommand(Guid.Empty));
	}

	[Fact]
	public void Command_WithValidId_ReturnsValidCommand()
	{
		Guid id = Guid.NewGuid();
		DeleteAccountCommand command = new(id);
		command.Id.Should().Be(id);
	}
}
