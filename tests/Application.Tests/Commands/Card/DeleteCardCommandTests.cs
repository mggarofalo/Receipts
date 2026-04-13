using Application.Commands.Card.Delete;
using FluentAssertions;

namespace Application.Tests.Commands.Card;

public class DeleteCardCommandTests
{
	[Fact]
	public void Command_WithEmptyId_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => new DeleteCardCommand(Guid.Empty));
	}

	[Fact]
	public void Command_WithValidId_ReturnsValidCommand()
	{
		Guid id = Guid.NewGuid();
		DeleteCardCommand command = new(id);
		command.Id.Should().Be(id);
	}
}
